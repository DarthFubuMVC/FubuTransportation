﻿using System;
using FubuCore;
using FubuCore.Dates;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Runtime.Invocation;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    public class TestContinuationContext : ContinuationContext
    {
        public TestContinuationContext() : base(new RecordingLogger(), new SettableClock(), MockRepository.GenerateMock<IChainInvoker>())
        {
            SystemTime.As<SettableClock>().LocalNow(DateTime.Today.AddHours(5));
        }

        public RecordingLogger RecordedLogs
        {
            get { return Logger.As<RecordingLogger>(); }
        }
    }
}