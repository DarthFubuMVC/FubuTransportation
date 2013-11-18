﻿using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;
using FubuTransportation.Runtime.Routing;
using System.Linq;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Configuration
{
    public class ChannelNode
    {
        public Accessor SettingAddress
        {
            get { return _settingAddress; }
            set
            {
                if (value.PropertyType != typeof (Uri))
                {
                    throw new ArgumentOutOfRangeException("SettingAddress", "Can only be a Uri property");
                }
                _settingAddress = value;
            }
        }

        public readonly IList<ISettingsAware> SettingsRules = new List<ISettingsAware>(); 

        public string Key { get; set; }

        public IScheduler Scheduler = TaskScheduler.Default();
        public bool Incoming = false;

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();
        private Accessor _settingAddress;

        public Uri Uri { get; set; }
        public IChannel Channel { get; set; }

        public string DefaultContentType { get; set; }

        // TODO -- don't like this.  Goofy.  
        public bool ForReplies { get; set; }

        public bool Publishes(Type type)
        {
            return Rules.Any(x => x.Matches(type));
        }
        
        public void ReadSettings(IServiceLocator services)
        {
            var settings = services.GetInstance(SettingAddress.OwnerType);
            Uri = (Uri) SettingAddress.GetValue(settings);

            SettingsRules.Each(x => x.ApplySettings(settings, this));
        }

        public string Protocol()
        {
            return Uri != null ? Uri.Scheme : null;
        }

        public void Accept(IChannelNodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Describe(IScenarioWriter writer)
        {
            writer.WriteLine(Key);
            using (writer.Indent())
            {
                if (Incoming)
                {
                    writer.WriteLine("Listens to {0} with {1}", Uri, Scheduler);
                }

                Rules.Each(x => x.Describe());
            }
        }

        public override string ToString()
        {
            return string.Format("Channel: {0}", Key);
        }

        // virtual for testing of course
        public virtual IHeaders Send(Envelope envelope, ChannelNode replyNode = null)
        {
            var clone = new NameValueHeaders();
            envelope.Headers.Keys().Each(key => clone[key] = envelope.Headers[key]);

            clone[Envelope.DestinationKey] = Uri.ToString();
            clone[Envelope.ChannelKey] = Key;

            if (replyNode != null)
            {
                clone[Envelope.ReplyUriKey] = replyNode.Channel.Address.ToString();
            }

            Channel.Send(envelope.Data, clone);

            return clone;
        }
    }

    
}