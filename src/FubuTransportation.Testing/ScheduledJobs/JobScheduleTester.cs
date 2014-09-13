﻿using System;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Persistence;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class JobScheduleTester
    {
        [Test]
        public void remove_obsolete_jobs()
        {
            var schedule = new JobSchedule(new[]
            {
                new JobStatus(typeof (AJob), DateTime.Today),
                new JobStatus(typeof (BJob), DateTime.Today),
            });

            schedule.RemoveObsoleteJobs(new Type[]{typeof(AJob), typeof(CJob)});
            schedule.Find(typeof (BJob)).Status.ShouldEqual(JobExecutionStatus.Inactive);
        }


    }
}