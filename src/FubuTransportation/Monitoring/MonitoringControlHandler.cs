﻿using System;
using System.Threading.Tasks;
using FubuCore.Logging;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class MonitoringControlHandler
    {
        private readonly IPersistentTaskController _controller;
        private readonly ILogger _logger;

        public MonitoringControlHandler(ISubscriptionRepository repository, IPersistentTaskController controller, ILogger logger)
        {
            _controller = controller;
            _logger = logger;
        }

        public Task<TaskHealthResponse> Handle(TaskHealthRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TaskDeactivationResponse> Handle(TaskDeactivation deactivation)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskDeactivation
    {
        public TaskDeactivation()
        {
        }

        public TaskDeactivation(Uri subject)
        {
            Subject = subject;
        }

        public Uri Subject { get; set; }

        protected bool Equals(TaskDeactivation other)
        {
            return Equals(Subject, other.Subject);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TaskDeactivation) obj);
        }

        public override int GetHashCode()
        {
            return (Subject != null ? Subject.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Deactivate Task: {0}", Subject);
        }
    }

    public class TaskDeactivationResponse
    {
        public Uri Subject { get; set; }
        public bool Success { get; set; }
    }
}