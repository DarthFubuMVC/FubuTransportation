﻿using System;
using System.Linq;
using System.Threading;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class when_scheduling_jobs
    {
        private JobSchedule theSchedule;
        private ScheduledJobGraph theGraph;
        private StubJobExecutor theExecutor;
        private readonly DateTimeOffset now = DateTime.Today.AddHours(-1);

        [SetUp]
        public void SetUp()
        {
            theExecutor = new StubJobExecutor();

            theSchedule = new JobSchedule(new []
            {
                JobStatus.For<AJob>(DateTime.Today), 
                JobStatus.For<BJob>(DateTime.Today.AddHours(1)), 
                JobStatus.For<CJob>(DateTime.Today.AddHours(2)), 
            });

            theGraph = new ScheduledJobGraph();
            theGraph.Jobs.Add(new ScheduledJob<BJob>(new DummyScheduleRule(DateTime.Today.AddHours(1))));
            theGraph.Jobs.Add(new ScheduledJob<CJob>(new DummyScheduleRule(DateTime.Today.AddHours(3))));
            theGraph.Jobs.Add(new ScheduledJob<DJob>(new DummyScheduleRule(DateTime.Today.AddHours(4))));
            theGraph.Jobs.Add(new ScheduledJob<EJob>(new DummyScheduleRule(DateTime.Today.AddHours(5))));
        
            // not that worried about pushing the time around
            theGraph.DetermineSchedule(now, theExecutor, theSchedule);
        }

        [Test]
        public void changes_the_jobs_that_are_already_scheduled_correcting_where_necessary()
        {
            theSchedule.Find(typeof (CJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(3));
            theSchedule.Find(typeof(BJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(1));
        }

        [Test]
        public void schedules_new_jobs()
        {
            theSchedule.Find(typeof(DJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(4));
            theSchedule.Find(typeof(EJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(5));
        }

        [Test]
        public void removes_obsolete_jobs()
        {
            theSchedule.Find(typeof(AJob)).Status.ShouldEqual(JobExecutionStatus.Inactive);
        }
    }

    public class DJob : IJob
    {
        public void Execute(CancellationToken cancellation)
        {
            throw new System.NotImplementedException();
        }
    }

    public class EJob : IJob
    {
        public void Execute(CancellationToken cancellation)
        {
            throw new System.NotImplementedException();
        }
    }
}