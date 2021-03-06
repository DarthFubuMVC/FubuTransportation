using System.Threading.Tasks;
using FubuTransportation.Polling;
using FubuTransportation.Runtime;
using FubuTransportation.ScheduledJobs.Persistence;

namespace FubuTransportation.ScheduledJobs.Execution
{
    public class ScheduledJobRunner<T> where T : IJob
    {
        private readonly T _job;
        private readonly IScheduleStatusMonitor _monitor;
        private readonly IScheduledJob<T> _scheduledJob;
        private readonly Envelope _envelope;

        public ScheduledJobRunner(T job, IScheduleStatusMonitor monitor, IScheduledJob<T> scheduledJob,
            Envelope envelope)
        {
            _job = job;
            _monitor = monitor;
            _scheduledJob = scheduledJob;
            _envelope = envelope;
        }

        public Task<RescheduleRequest<T>> Execute(ExecuteScheduledJob<T> request)
        {
            var tracker = _monitor.TrackJob(_envelope.Attempts, _job);
            return _scheduledJob.ToTask(_job, tracker);
        }
    }
}