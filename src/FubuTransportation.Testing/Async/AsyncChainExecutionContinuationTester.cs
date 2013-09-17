﻿using FubuCore;
using FubuTransportation.Async;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Async
{
    [TestFixture]
    public class AsyncChainExecutionContinuationTester
    {
        [Test]
        public void executing()
        {
            var envelope = ObjectMother.Envelope();
            var context = new TestContinuationContext();

            var inner = MockRepository.GenerateMock<IContinuation>();

            var continuation = new AsyncChainExecutionContinuation(() => inner);
            continuation.Execute(envelope, context);

            continuation.Task.Wait(1.Seconds());

            inner.AssertWasCalled(x => x.Execute(envelope, context));
        }
    }
}