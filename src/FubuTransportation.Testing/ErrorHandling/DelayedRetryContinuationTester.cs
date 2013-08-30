﻿using FubuTransportation.ErrorHandling;
using NUnit.Framework;
using FubuCore;
using FubuTestingSupport;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ErrorHandling
{
    [TestFixture]
    public class DelayedRetryContinuationTester
    {
        [Test]
        public void do_as_a_delay_w_the_timespan_given()
        {
            var continuation = new DelayedRetryContinuation(5.Minutes());
            var context = new TestContinuationContext();

            var envelope = ObjectMother.Envelope();

            continuation.Execute(envelope, context);

            envelope.ExecutionTime.Value.ShouldEqual(context.SystemTime.UtcNow().AddMinutes(5));

            envelope.Callback.AssertWasCalled(x => x.MoveToDelayed());
        }
    }
}