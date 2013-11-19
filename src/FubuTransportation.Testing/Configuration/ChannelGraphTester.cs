﻿using System;
using FubuCore;
using FubuCore.Logging;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.Scheduling;
using NUnit.Framework;
using FubuTestingSupport;
using System.Collections.Generic;
using Rhino.Mocks;
using FubuCore.Reflection;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class ChannelGraphTester
    {
        [Test]
        public void the_default_content_type_should_be_xml_serialization()
        {
            new ChannelGraph().DefaultContentType.ShouldEqual(new XmlMessageSerializer().ContentType);
        }

        [Test]
        public void to_key_by_expression()
        {
            ChannelGraph.ToKey<ChannelSettings>(x => x.Outbound)
                        .ShouldEqual("Channel:Outbound");
        }

        [Test]
        public void channel_for_by_accessor()
        {
            var graph = new ChannelGraph();
            var channelNode = graph.ChannelFor<ChannelSettings>(x => x.Outbound);
            channelNode
                 .ShouldBeTheSameAs(graph.ChannelFor<ChannelSettings>(x => x.Outbound));


            channelNode.Key.ShouldEqual("Channel:Outbound");
            channelNode.SettingAddress.Name.ShouldEqual("Outbound");

        }

        [Test]
        public void reading_settings()
        {
            var channel = new ChannelSettings
            {
                Outbound = new Uri("channel://outbound"),
                Downstream = new Uri("channel://downstream")
            };

            var bus = new BusSettings
            {
                Outbound = new Uri("bus://outbound"),
                Downstream = new Uri("bus://downstream")
            };

            var services = new InMemoryServiceLocator();
            services.Add(channel);
            services.Add(bus);

            var graph = new ChannelGraph();
            graph.ChannelFor<ChannelSettings>(x => x.Outbound);
            graph.ChannelFor<ChannelSettings>(x => x.Downstream);
            graph.ChannelFor<BusSettings>(x => x.Outbound);
            graph.ChannelFor<BusSettings>(x => x.Downstream);

            graph.ReadSettings(services);

            graph.ChannelFor<ChannelSettings>(x => x.Outbound)
                 .Uri.ShouldEqual(channel.Outbound);
            graph.ChannelFor<ChannelSettings>(x => x.Downstream)
                 .Uri.ShouldEqual(channel.Downstream);
            graph.ChannelFor<BusSettings>(x => x.Outbound)
                .Uri.ShouldEqual(bus.Outbound);
            graph.ChannelFor<BusSettings>(x => x.Downstream)
                .Uri.ShouldEqual(bus.Downstream);
        }

        [Test]
        public void start_receiving()
        {
            using (var graph = new ChannelGraph())
            {
                var node1 = graph.ChannelFor<ChannelSettings>(x => x.Upstream);
                var node2 = graph.ChannelFor<ChannelSettings>(x => x.Downstream);
                var node3 = graph.ChannelFor<BusSettings>(x => x.Upstream);
                var node4 = graph.ChannelFor<BusSettings>(x => x.Downstream);

                node1.Incoming = true;
                node2.Incoming = false;
                node3.Incoming = true;
                node4.Incoming = false;

                graph.Each(x => x.Channel = MockRepository.GenerateMock<IChannel>());

                graph.StartReceiving(MockRepository.GenerateMock<IHandlerPipeline>());

                node1.Channel.AssertWasCalled(x => x.Receive(null), x => x.IgnoreArguments());
                node2.Channel.AssertWasNotCalled(x => x.Receive(null), x => x.IgnoreArguments());
                node3.Channel.AssertWasCalled(x => x.Receive(null), x => x.IgnoreArguments());
                node4.Channel.AssertWasNotCalled(x => x.Receive(null), x => x.IgnoreArguments());

                
            }
        }

        [Test]
        public void channel_graph_has_to_be_application_level()
        {
            typeof(ChannelGraph).HasAttribute<ApplicationLevelAttribute>().ShouldBeTrue();
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }

        public int UpstreamCount { get; set; }
        public int OutboundCount { get; set; }
    }

    public class BusSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}