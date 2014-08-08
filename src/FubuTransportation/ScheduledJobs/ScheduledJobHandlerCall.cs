﻿using System.Reflection;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobHandlerCall<T> : HandlerCall where T : IJob
    {
        private static MethodInfo method()
        {
            return ReflectionHelper.GetMethod<ScheduledJobRunner<T>>(x => x.Execute(null));
        }

        public ScheduledJobHandlerCall()
            : base(typeof(ScheduledJobRunner<T>), method())
        {
        }
    }
}