﻿using System;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs.Persistence
{
    public interface IScheduleStatusMonitor
    {
        void Persist(Action<JobSchedule> scheduling);

        void MarkScheduled<T>(DateTimeOffset nextTime);
        IJobRunTracker TrackJob<T>(int attempts, T job) where T : IJob;
    }

    public interface IJobRunTracker
    {
        void Success(DateTimeOffset nextTime);
        void Failure(Exception ex);
        DateTimeOffset Now();
    }
}