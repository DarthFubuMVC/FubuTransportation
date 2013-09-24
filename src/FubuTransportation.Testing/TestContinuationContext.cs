﻿using System;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Runtime.Cascading;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Testing.Runtime;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    public class TestContinuationContext : ContinuationContext
    {


        public TestContinuationContext() : base(new RecordingLogger(), new SettableClock(), MockRepository.GenerateMock<IChainInvoker>(), new RecordingEnvelopeSender())
        {
            SystemTime.As<SettableClock>().LocalNow(DateTime.Today.AddHours(5));
        }

        public RecordingEnvelopeSender RecordedOutgoing
        {
            get
            {
                return Outgoing.As<RecordingEnvelopeSender>();
            }
        }

        public RecordingLogger RecordedLogs
        {
            get { return Logger.As<RecordingLogger>(); }
        }
    }
}