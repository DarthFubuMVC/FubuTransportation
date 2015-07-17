using System;
using FubuTransportation.ScheduledJobs.Persistence;

namespace FubuTransportation.ScheduledJobs.Execution
{
    // This won't work very well with nodes in different time zones
    public class EveryDayAtSpecificTime : IScheduleRule
    {
        private readonly int _hour;
        private readonly int _minute;
        private readonly int _gracePeriodInMinutes;

        public EveryDayAtSpecificTime(int hour, int minute, int gracePeriodInMinutes = 15)
        {
            _hour = hour;
            _minute = minute;
            _gracePeriodInMinutes = gracePeriodInMinutes;
        }

        public DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime, JobExecutionRecord lastExecution)
        {
            var localcurrentTime = currentTime.ToLocalTime();
            var nextScheduledTime  = new DateTime(localcurrentTime.Year, localcurrentTime.Month, localcurrentTime.Day, _hour, _minute, 0, 0, DateTimeKind.Local);
            var gracePeriodTime = nextScheduledTime.AddMinutes(_gracePeriodInMinutes);

            var scheduledTimeHasAlreadyPastToday = nextScheduledTime < localcurrentTime;
            var withinGracePeriodRightNow = nextScheduledTime < localcurrentTime && localcurrentTime <= gracePeriodTime;

            var lastExecutionWithinGracePeriod = false;
            if (lastExecution != null)
            {
                var lastExecutionLocalTime = lastExecution.Finished.ToLocalTime();
                lastExecutionWithinGracePeriod = nextScheduledTime < lastExecutionLocalTime && lastExecutionLocalTime <= gracePeriodTime;
            }

            if (scheduledTimeHasAlreadyPastToday && (!withinGracePeriodRightNow || lastExecutionWithinGracePeriod))
            {
                // Switch to tomorrow
                nextScheduledTime = nextScheduledTime.AddDays(1);
            }

            return nextScheduledTime.ToUniversalTime();
        }
    }

    /*
    public class EveryDayAt1Am : EveryDayAtSpecificTime
    {
        public EveryDayAt1Am() : base(hour: 01, minute: 00)
        {}
    }
    */
}
