﻿using System.Threading;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class PollingJobRunImmediatelyIntegrationTester
    {
        private FubuRuntime theRuntime;

        [TestFixtureSetUp]
        public void SetUp()
        {
            ImmediateJob.Executed = DelayJob.Executed = 0;

            var container = new Container();
            theRuntime = FubuTransport.For<PollingImmediateRegistry>()
                                      .StructureMap(container)
                                      .Bootstrap();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            theRuntime.Dispose();
        }

        [Test]
        public void should_only_execute_ImmediateJob_now_and_interval_should_still_work()
        {
            DelayJob.Executed.ShouldEqual(0);
            ImmediateJob.Executed.ShouldEqual(1);

            Wait.Until(() => ImmediateJob.Executed > 1, timeoutInMilliseconds: 6000);
            ImmediateJob.Executed.ShouldBeGreaterThan(1);
        }
    }

    public class PollingImmediateRegistry : FubuTransportRegistry
    {
        public PollingImmediateRegistry()
        {
            EnableInMemoryTransport();

            Polling.RunJob<ImmediateJob>().ScheduledAtInterval<PollingImmediateSettings>(x => x.ImmediateInterval).RunImmediately();
            Polling.RunJob<DelayJob>().ScheduledAtInterval<PollingImmediateSettings>(x => x.DelayInterval);
        }
    }

    public class PollingImmediateSettings
    {
        public PollingImmediateSettings()
        {
            // Make these sufficiently high to allow bootstrapping to finish before any jobs hit their timer
            ImmediateInterval = 5000;
            DelayInterval = 5000;
        }

        public double ImmediateInterval { get; set; }
        public double DelayInterval { get; set; }
    }

    public class ImmediateJob : IJob
    {
        public static int Executed = 0;

        public void Execute(CancellationToken token)
        {
            Executed++;
        }
    }

    public class DelayJob : IJob
    {
        public static int Executed = 0;

        public void Execute(CancellationToken token)
        {
            Executed++;
        }
    }
}
