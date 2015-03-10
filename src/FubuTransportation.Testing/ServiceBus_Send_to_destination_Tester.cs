﻿using System;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Events;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ServiceBus_Send_to_destination_Tester : InteractionContext<ServiceBus>
    {
        private Envelope theLastEnvelopeSent
        {
            get
            {
                return MockFor<IEnvelopeSender>().GetArgumentsForCallsMadeOn(x => x.Send(null))
                    .Last()[0].As<Envelope>();
            }
        }

        [Test]
        public void sends_to_appropriate_destination()
        {
            var destination = new Uri("memory://blah");
            var message = new Message1();
            
            ClassUnderTest.Send(destination, message);

            theLastEnvelopeSent.Destination.ShouldEqual(destination);
            theLastEnvelopeSent.Message.ShouldBeTheSameAs(message);
        }

        [Test]
        public void sends_to_appropriate_destination_and_waits()
        {
            var destination = new Uri("memory://blah");
            var message = new Message1();
            
            ClassUnderTest.SendAndWait(destination, message).ShouldNotBeNull();

            theLastEnvelopeSent.Destination.ShouldEqual(destination);
            theLastEnvelopeSent.Message.ShouldBeTheSameAs(message);

            var lastReplyListener = MockFor<IEventAggregator>().GetArgumentsForCallsMadeOn(x => x.AddListener(null))
                .Last()[0].As<ReplyListener<Acknowledgement>>();
            lastReplyListener.IsExpired.ShouldBeFalse();
            MockFor<IEventAggregator>().AssertWasCalled(x => x.AddListener(Arg<ReplyListener<Acknowledgement>>.Is.Anything));
        }
    }
}