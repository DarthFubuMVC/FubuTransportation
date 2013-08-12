﻿using FubuTestingSupport;
using FubuTransportation.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Logging
{
    [TestFixture]
    public class EventAggregationListenerTester : InteractionContext<EventAggregationListener>
    {
        [Test]
        public void listens_to_events_in_FubuTransportation()
        {
            ClassUnderTest.ListensFor(typeof(ChainExecutionFinished)).ShouldBeTrue();
            ClassUnderTest.ListensFor(typeof(ChainExecutionStarted)).ShouldBeTrue();
        }

        [Test]
        public void does_not_listen_to_events_outside_of_FubuTransportation()
        {
            ClassUnderTest.ListensFor(GetType()).ShouldBeFalse();
        }

        [Test]
        public void send_debug_message()
        {
            var message = new ChainExecutionFinished();

            ClassUnderTest.DebugMessage(message);

            MockFor<IEventAggregator>().AssertWasCalled(x => x.SendMessage(message));
        }


        [Test]
        public void send_info_message()
        {
            var message = new ChainExecutionFinished();

            ClassUnderTest.InfoMessage(message);

            MockFor<IEventAggregator>().AssertWasCalled(x => x.SendMessage(message));
        }
    }

}