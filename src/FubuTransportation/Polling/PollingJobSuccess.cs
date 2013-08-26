﻿using FubuCore.Logging;

namespace FubuTransportation.Polling
{
    public class PollingJobSuccess : LogRecord
    {
        public string Description { get; set; }

        protected bool Equals(PollingJobSuccess other)
        {
            return string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PollingJobSuccess) obj);
        }

        public override int GetHashCode()
        {
            return (Description != null ? Description.GetHashCode() : 0);
        }
    }
}