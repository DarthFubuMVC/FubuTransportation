﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class when_handling_subscriptions_changed : InteractionContext<SubscriptionsHandler>
    {
        private Subscription[] theSubscriptions;
        private ChannelGraph theGraph;

        protected override void beforeEach()
        {
            theSubscriptions = new Subscription[]
            {
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription()
            };

            theGraph = new ChannelGraph {Name = "TheNode"};
            Services.Inject(theGraph);

            MockFor<ISubscriptionRepository>().Stub(x => x.LoadSubscriptions())
                .Return(theSubscriptions);

            ClassUnderTest.Handle(new SubscriptionsChanged());
        }

        [Test]
        public void should_load_the_new_subscriptions_into_the_running_cache()
        {
            MockFor<ISubscriptionCache>()
                .AssertWasCalled(x => x.LoadSubscriptions(theSubscriptions));
        }
    }

    [TestFixture]
    public class when_sending_a_subscription_to_a_peer : InteractionContext<SubscriptionsHandler>
    {
        private RecordingEnvelopeSender theSender;
        private TransportNode thePeer;

        protected override void beforeEach()
        {
            theSender = new RecordingEnvelopeSender();
            Services.Inject<IEnvelopeSender>(theSender);

            thePeer = new TransportNode
            {
                Addresses = new Uri[] {"memory://replies".ToUri()}
            };

            ClassUnderTest.SendSubscriptionChangedToPeer(thePeer);
        }

        [Test]
        public void should_have_sent_a_message_to_the_peer_to_reload_subscriptions()
        {
            var envelope = theSender.Sent.Single();
            envelope.Message.ShouldBeOfType<SubscriptionsChanged>();
            envelope.Destination.ShouldEqual(thePeer.Addresses.FirstOrDefault());
        }
    }

    [TestFixture]
    public class when_handling_the_subscriptions_changed_message : InteractionContext<SubscriptionsHandler>
    {
        private TransportNode[] thePeers;
        private SubscriptionRequested theMessage;

        protected override void beforeEach()
        {
            Services.PartialMockTheClassUnderTest();

            thePeers = new TransportNode[]
            {
                new TransportNode(),
                new TransportNode(),
                new TransportNode(),
            };

            ClassUnderTest.Expect(x => x.ReloadSubscriptions());
            thePeers.Each(peer => ClassUnderTest.Expect(o => o.SendSubscriptionChangedToPeer(peer)));

            MockFor<ISubscriptionRepository>().Stub(x => x.FindPeers())
                .Return(thePeers);

            theMessage = new SubscriptionRequested
            {
                Subscriptions = new Subscription[]
                {
                    ObjectMother.ExistingSubscription(),
                    ObjectMother.ExistingSubscription(),
                    ObjectMother.ExistingSubscription(),
                    ObjectMother.ExistingSubscription(),
                    ObjectMother.ExistingSubscription()
                }
            };

            ClassUnderTest.Handle(theMessage);
        }

        [Test]
        public void should_persist_the_new_subscriptions()
        {
            MockFor<ISubscriptionRepository>()
                .AssertWasCalled(x => x.PersistRequirements(theMessage.Subscriptions));
        }

        [Test]
        public void should_reload_subscriptions()
        {
            ClassUnderTest.AssertWasCalled(x => x.ReloadSubscriptions());
        }

        [Test]
        public void should_message_all_of_its_peers_to_reload_their_subscriptions()
        {
            thePeers.Each(peer => {
                ClassUnderTest.AssertWasCalled(x => x.SendSubscriptionChangedToPeer(peer));
            });
        }
    }
}