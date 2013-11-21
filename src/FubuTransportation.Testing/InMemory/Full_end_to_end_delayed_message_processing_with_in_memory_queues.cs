﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bottles.Services.Messaging.Tracking;
using FubuCore;
using FubuCore.Dates;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class Full_end_to_end_delayed_message_processing_with_in_memory_queues
    {
        private IServiceBus theServiceBus;
        private SettableClock theClock;
        private OneMessage message1;
        private OneMessage message2;
        private OneMessage message3;
        private OneMessage message4;
        private FubuRuntime runtime;

        [TestFixtureSetUp]
        public void SetUp()
        {
            // Need to do something about this.  Little ridiculous
            FubuTransport.SetupForInMemoryTesting();
            TestMessageRecorder.Clear();
            MessageHistory.ClearAll();
            InMemoryQueueManager.ClearAll();

            runtime = FubuTransport.For<DelayedRegistry>().StructureMap(new Container())
                                       .Bootstrap();

            theServiceBus = runtime.Factory.Get<IServiceBus>();

            theClock = runtime.Factory.Get<ISystemTime>().As<SettableClock>();

            message1 = new OneMessage();
            message2 = new OneMessage();
            message3 = new OneMessage();
            message4 = new OneMessage();

            theServiceBus.DelaySend(message1, theClock.UtcNow().AddHours(1));
            theServiceBus.DelaySend(message2, theClock.UtcNow().AddHours(1));
            theServiceBus.DelaySend(message3, theClock.UtcNow().AddHours(2));
            theServiceBus.DelaySend(message4, theClock.UtcNow().AddHours(2));

        }

        [Test]
        public void things_are_received_at_the_right_times()
        {
            TestMessageRecorder.AllProcessed.Any().ShouldBeFalse();

            Thread.Sleep(2000);
            TestMessageRecorder.AllProcessed.Any().ShouldBeFalse();

            theClock.LocalNow(theClock.LocalTime().Add(61.Minutes()));

            Wait.Until(() => TestMessageRecorder.HasProcessed(message1)).ShouldBeTrue();
            Wait.Until(() => TestMessageRecorder.HasProcessed(message2)).ShouldBeTrue();

            TestMessageRecorder.HasProcessed(message3).ShouldBeFalse();
            TestMessageRecorder.HasProcessed(message4).ShouldBeFalse();

            theClock.LocalNow(theClock.LocalTime().Add(61.Minutes()));

            Wait.Until(() => TestMessageRecorder.HasProcessed(message3)).ShouldBeTrue();
            Wait.Until(() => TestMessageRecorder.HasProcessed(message4)).ShouldBeTrue();

            // If it's more than this, we got problems
            TestMessageRecorder.AllProcessed.Count().ShouldEqual(4);
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            runtime.SafeDispose();

            // TODO - HAVE TO DISPOSE THE RUNTIME!!!!!!!
            FubuTransport.Reset();
        }
    }
}