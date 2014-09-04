﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class TransportNodeTester
    {
        [Test]
        public void build_with_channel_graph_sets_the_id_to_the_node_id()
        {
            var graph = new ChannelGraph
            {
                NodeId = "Foo@Bar"
                
            };

            graph.AddReplyChannel(InMemoryChannel.Protocol, "memory://localhost/replies".ToUri());

            var node = new TransportNode(graph);

            node.Id.ShouldEqual(graph.NodeId);
        }

        [Test]
        public void blow_up_if_no_reply_channels()
        {
            var graph = new ChannelGraph
            {
                NodeId = "Foo@Bar"

            };

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                new TransportNode(graph);
            }).Message.ShouldEqual("At least one reply channel is required");
        }

        [Test]
        public void create_a_transport_node_from_a_channel_graph()
        {
            var graph = new ChannelGraph
            {
                Name = "Service1"
            };

            graph.AddReplyChannel("memory", "memory://replies".ToUri());
            graph.AddReplyChannel("foo", "foo://replies".ToUri());
            graph.AddReplyChannel("bar", "bar://replies".ToUri());

            var node = new TransportNode(graph);

            node.NodeName.ShouldEqual("Service1");

            node.Addresses.OrderBy(x => x.ToString()).ShouldHaveTheSameElementsAs("bar://replies".ToUri(), "foo://replies".ToUri(), "memory://replies".ToUri());
        }

        
    }
}