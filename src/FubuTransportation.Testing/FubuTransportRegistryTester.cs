﻿using System;
using System.Linq.Expressions;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportRegistryTester
    {
        [SetUp]
        public void SetUp()
        {
            theRegistry = new BusRegistry();
            _runtime = new Lazy<FubuRuntime>(() => {
                return FubuTransport.For(theRegistry).StructureMap(new Container()).Bootstrap();
            });

            _handlers = new Lazy<HandlerGraph>(() => _runtime.Value.Factory.Get<HandlerGraph>());
            _channels = new Lazy<ChannelGraph>(() => _runtime.Value.Factory.Get<ChannelGraph>());
        }

        private BusRegistry theRegistry;
        private Lazy<HandlerGraph> _handlers;
        private Lazy<ChannelGraph> _channels;
        private Lazy<FubuRuntime> _runtime;
    
    
        public HandlerGraph theHandlers
        {
            get { return _handlers.Value; }
        }

        public ChannelGraph theChannels
        {
            get { return _channels.Value; }
        }

        public ChannelNode channelFor(Expression<Func<BusSettings, Uri>> expression)
        {
            return theChannels.ChannelFor(expression);
        }


        [Test]
        public void set_the_default_content_type_by_serializer_type()
        {
            theRegistry.DefaultSerializer<BinarySerializer>();

            theChannels.DefaultContentType.ShouldEqual(new BinarySerializer().ContentType);
        }

        [Test]
        public void set_the_default_content_type_by_string()
        {
            theRegistry.DefaultContentType("application/json");
            theChannels.DefaultContentType.ShouldEqual("application/json");
        }

        [Test]
        public void set_the_default_content_type_for_a_channel_by_serializer()
        {
            theRegistry.Channel(x => x.Outbound).DefaultSerializer<BinarySerializer>();
            theRegistry.Channel(x => x.Downstream).DefaultSerializer<XmlMessageSerializer>();
            theRegistry.Channel(x => x.Upstream).DefaultSerializer<BasicJsonMessageSerializer>();

            channelFor(x => x.Outbound).DefaultContentType.ShouldEqual(new BinarySerializer().ContentType);
            channelFor(x => x.Downstream).DefaultContentType.ShouldEqual(new XmlMessageSerializer().ContentType);
            channelFor(x => x.Upstream).DefaultContentType.ShouldEqual(new BasicJsonMessageSerializer().ContentType);
        }

        [Test]
        public void set_the_default_content_type_for_a_channel_by_string()
        {
            theRegistry.Channel(x => x.Outbound).DefaultContentType("application/json");
            channelFor(x => x.Outbound).DefaultContentType.ShouldEqual("application/json");
        }
    }

    public class BusSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }

    public class BusRegistry : FubuTransportRegistry<BusSettings>
    {
        public BusRegistry()
        {

        }
    }
}