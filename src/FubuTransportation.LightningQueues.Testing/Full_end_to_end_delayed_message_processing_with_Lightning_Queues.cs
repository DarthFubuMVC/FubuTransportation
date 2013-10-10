﻿using System.IO;
using System.Linq;
using System.Threading;
using FubuCore;
using FubuCore.Dates;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Testing;
using FubuTransportation.Testing.InMemory;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class Full_end_to_end_delayed_message_processing_with_Lightning_Queues
    {
        private FubuRuntime _runtime;
        private IServiceBus theServiceBus;
        private SettableClock theClock;
        private OneMessage message1;
        private OneMessage message2;
        private OneMessage message3;
        private OneMessage message4;

        [TestFixtureSetUp]
        public void SetUp()
        {
            FubuTransport.Reset();

            // Need to do something about this.  Little ridiculous
            var settings = new BusSettings
            {
                Downstream = "lq.tcp://localhost:2020/downstream".ToUri()
            };


            var container = new Container();
            container.Inject(settings);

            _runtime = FubuTransport.For<DelayedRegistry>().StructureMap(container)
                                       .Bootstrap();

            theServiceBus = _runtime.Factory.Get<IServiceBus>();
            _runtime.Factory.Get<IPersistentQueues>().ClearAll();

            theClock = _runtime.Factory.Get<ISystemTime>().As<SettableClock>();

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
            FubuTransport.Reset();
            _runtime.Dispose();
        }
    }
}