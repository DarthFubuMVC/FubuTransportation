﻿using System;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerChain_Exception_Handling_Rules_Registration_Tester
    {
        [Test]
        public void retry_now()
        {
            var chain = new HandlerChain();

            chain.OnException<NotImplementedException>()
                .Retry();

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotImplementedException>>();
            handler.Continuation().ShouldBeOfType<RetryNowContinuation>();
        }

        [Test]
        public void requeue()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .Requeue();

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotSupportedException>>();
            handler.Continuation().ShouldBeOfType<RequeueContinuation>();
        }

        [Test]
        public void move_to_error_queue()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .MoveToErrorQueue();

            chain.ErrorHandlers.Single().ShouldBeOfType<MoveToErrorQueueHandler<NotSupportedException>>();
        }

        [Test]
        public void retry_later()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .RetryLater(10.Minutes());

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotSupportedException>>();
            handler.Continuation().ShouldBeOfType<DelayedRetryContinuation>()
                .Delay.ShouldEqual(10.Minutes());
        }

        [Test]
        public void add_multiple_continuations()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .RetryLater(10.Minutes())
                .Then
                .ContinueWith<TellTheSenderHeSentSomethingWrong>();

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            var continuation = handler.Continuation().ShouldBeOfType<CompositeContinuation>();
            continuation.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(DelayedRetryContinuation), typeof(TellTheSenderHeSentSomethingWrong));

        }

        [Test]
        public void respond_with_message()
        {
            var chain = new HandlerChain();

            chain.OnException<NotImplementedException>()
                .RespondWithMessage((ex, env) => new object());

            chain.ErrorHandlers.Single().ShouldBeOfType<RespondWithMessageHandler<NotImplementedException>>();
        }
    }

    public class TellTheSenderHeSentSomethingWrong : IContinuation
    {
        public void Execute(Envelope envelope, ContinuationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
