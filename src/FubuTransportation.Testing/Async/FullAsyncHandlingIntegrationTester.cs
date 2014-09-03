﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;

namespace FubuTransportation.Testing.Async
{
    [TestFixture]
    public class FullAsyncHandlingIntegrationTester
    {
        [Test]
        public void ordered_the_way_we_expect_it_to_be()
        {
            AsyncWatcher.Clear();

            using (var runtime = FubuTransport.For<AsyncRegistry>().StructureMap().Bootstrap())
            {
                var invoker = runtime.Factory.Get<IChainInvoker>();
                var message = new Foo {Name = "Buck Rogers"};

                invoker.InvokeNow(message);

                Wait.Until(() => AsyncWatcher.Messages.Count() == 4);

                AsyncWatcher.Messages.ElementAt(0).ShouldEqual("wrapper:start");
                AsyncWatcher.Messages.ElementAt(1).ShouldEndWith("Buck Rogers");
                AsyncWatcher.Messages.ElementAt(2).ShouldEndWith("Buck Rogers");
                AsyncWatcher.Messages.ElementAt(3).ShouldEndWith("wrapper:finish");
            }
        }
    }

    public class AsyncRegistry : FubuTransportRegistry
    {
        public AsyncRegistry()
        {
            EnableInMemoryTransport();

            Local.WrapWith<FooWrapper>();
        }
    }

    public static class AsyncWatcher
    {
        private readonly static object _locker = new object();

        public static IEnumerable<string> Messages
        {
            get { return _messages; }
        }

        public static void Write(string message)
        {
            lock (_locker)
            {
                _messages.Add(message);
            }
        }

        private static readonly IList<string> _messages = new List<string>();

        public static void Clear()
        {
            _messages.Clear();
        }
    }

    public class Foo
    {
        public string Name;
    }

    public class FooWrapper : WrappingBehavior
    {
        protected override void invoke(Action action)
        {
            AsyncWatcher.Write("wrapper:start");
            action();
            AsyncWatcher.Write("wrapper:finish");
        }
    }

    public class AsyncTestHandler
    {
        public Task Go(Foo foo)
        {
            return Task.Factory.StartNew(() => {
                Thread.Sleep(100);
                AsyncWatcher.Write("go:" + foo.Name);
            });
        }

        public Task Other(Foo foo)
        {
            return Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                AsyncWatcher.Write("other:" + foo.Name);
            });
        }
    }
}