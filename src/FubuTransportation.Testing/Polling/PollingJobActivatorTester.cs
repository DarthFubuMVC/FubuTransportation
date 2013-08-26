﻿using System;
using Bottles;
using Bottles.Diagnostics;
using FubuTestingSupport;
using FubuTransportation.Polling;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class PollingJobActivatorTester : InteractionContext<PollingJobActivator>
    {
        private IPollingJob[] theJobs;
        private NotImplementedException ex1;
        private NotSupportedException ex2;
        private IPackageLog theLog;
        private IPackageInfo[] thePackages;

        protected override void beforeEach()
        {
            theLog = MockFor<IPackageLog>();
            thePackages = new IPackageInfo[0];

            theJobs = Services.CreateMockArrayFor<IPollingJob>(5);
            Services.Inject<IPollingJobs>(new PollingJobs(theJobs));

            ex1 = new NotImplementedException();
            ex2 = new NotSupportedException();

            theJobs[1].Expect(x => x.Start()).Throw(ex1);
            theJobs[2].Expect(x => x.Start()).Throw(ex2);

            ClassUnderTest.Activate(thePackages, theLog);
        }

        [Test]
        public void should_start_all_the_jobs()
        {
            theJobs.Each(x => x.AssertWasCalled(job => job.Start()));
        }

        [Test]
        public void should_log_failures_for_both_of_the_failing_jobs()
        {
            theLog.AssertWasCalled(x => x.MarkFailure(ex1));
            theLog.AssertWasCalled(x => x.MarkFailure(ex2));
        }
    }

    [TestFixture]
    public class PollingJobDeactivatorTester : InteractionContext<PollingJobDeactivator>
    {
        private IPollingJob[] theJobs;
        private NotImplementedException ex1;
        private NotSupportedException ex2;
        private IPackageLog theLog;

        protected override void beforeEach()
        {
            theLog = MockFor<IPackageLog>();

            theJobs = Services.CreateMockArrayFor<IPollingJob>(5);
            Services.Inject<IPollingJobs>(new PollingJobs(theJobs));

            ex1 = new NotImplementedException();
            ex2 = new NotSupportedException();

            theJobs[1].Expect(x => x.Stop()).Throw(ex1);
            theJobs[2].Expect(x => x.Stop()).Throw(ex2);

            ClassUnderTest.Deactivate(theLog);
        }

        [Test]
        public void should_stop_all_the_jobs()
        {
            theJobs.Each(x => x.AssertWasCalled(job => job.Stop()));
        }

        [Test]
        public void should_log_failures_for_both_of_the_failing_jobs()
        {
            theLog.AssertWasCalled(x => x.MarkFailure(ex1));
            theLog.AssertWasCalled(x => x.MarkFailure(ex2));
        }
    }
}