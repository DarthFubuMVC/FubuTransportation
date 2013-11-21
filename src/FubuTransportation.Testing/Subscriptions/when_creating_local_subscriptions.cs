﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class when_creating_local_subscriptions
    {
        public BusSettings theSettings = InMemoryTransport.ToInMemory<BusSettings>();
        private ChannelGraph theGraph;
        private Uri theLocalReplyUri;
        private IEnumerable<Subscription> theSubscriptions;

        [SetUp]
        public void SetUp()
        {
            theGraph = new ChannelGraph { Name = "FooNode" };

            var requirement = new LocalSubscriptionRequirement<BusSettings>(x => x.Upstream);
            requirement.AddType(typeof(FooMessage));
            requirement.AddType(typeof(BarMessage));

            theLocalReplyUri = InMemoryTransport.ReplyUriForGraph(theGraph);

            theGraph.AddReplyChannel("Fake2", "fake2://2".ToUri());
            theGraph.AddReplyChannel(InMemoryChannel.Protocol, theLocalReplyUri);
            theGraph.AddReplyChannel("Fake1", "fake1://1".ToUri());

            theSubscriptions = requirement.Determine(theSettings, theGraph);
        }

        [Test]
        public void should_set_the_receiver_uri_to_the_reply_uri_of_the_matching_transport()
        {
            theSubscriptions.First().Receiver
                .ShouldEqual(theLocalReplyUri);
        }

        [Test]
        public void sets_the_node_name_from_the_channel_graph()
        {
            theSubscriptions.Select(x => x.NodeName).Distinct()
                .Single().ShouldEqual(theGraph.Name);
        }

        [Test]
        public void should_set_the_source_uri_to_the_requested_source_from_settings()
        {
            theSubscriptions.First().Source
                .ShouldEqual(theSettings.Upstream);

        }

        [Test]
        public void should_add_a_subscription_for_each_type()
        {
            theSubscriptions.Select(x => x.MessageType)
                .ShouldHaveTheSameElementsAs(typeof(FooMessage).GetFullName(), typeof(BarMessage).GetFullName());
        }


        public class BusSettings
        {
            public Uri Outbound { get; set; }
            public Uri Downstream { get; set; }
            public Uri Upstream { get; set; }
        }

        public class FooMessage{}
        public class BarMessage{}
    }


}