﻿
using System;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class EnvelopeContinuationChosen : LogRecord
    {
        public EnvelopeToken Envelope;
        public Type HandlerType;
        public Type ContinuationType;

        protected bool Equals(EnvelopeContinuationChosen other)
        {
            return Equals(Envelope, other.Envelope) && Equals(HandlerType, other.HandlerType) && Equals(ContinuationType, other.ContinuationType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EnvelopeContinuationChosen) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Envelope != null ? Envelope.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (HandlerType != null ? HandlerType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ContinuationType != null ? ContinuationType.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "Chose continuation {0} for envelope {1}".ToFormat(ContinuationType, Envelope);
        }
    }
}