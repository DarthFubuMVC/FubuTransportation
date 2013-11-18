﻿using System.Data;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace FubuTransportation.Testing.ErrorHandling
{
    [TestFixture]
    public class RetryNow_integration_testing
    {
        [Test]
        public void successfully_retries_now()
        {
            MessageThatBombsHandler.Throws = 2;
            MessageThatBombsHandler.Attempts = 0;
            MessageThatBombsHandler.Successful = null;

            using (var runtime = FubuTransport.For<RetryNoOnDbConcurrencyRegistry>()
                        .StructureMap(new Container())
                        .Bootstrap())
            {
                var pipeline = runtime.Factory.Get<IHandlerPipeline>();
                pipeline.Invoke(new Envelope {Message = new MessageThatBombs(), Callback = MockRepository.GenerateMock<IMessageCallback>()});
            }

            MessageThatBombsHandler.Successful.ShouldNotBeNull();
            MessageThatBombsHandler.Attempts.ShouldBeGreaterThan(1);
        }
    }

    public class RetryNoOnDbConcurrencyRegistry : FubuTransportRegistry
    {
        public RetryNoOnDbConcurrencyRegistry()
        {
            EnableInMemoryTransport();
            Local.Policy<RetryNowOnDbConcurrencyException>();
        }
    }


    public class RetryNowOnDbConcurrencyException : HandlerChainPolicy
    {
        public override void Configure(HandlerChain handlerChain)
        {
            handlerChain.MaximumAttempts = 5;
            handlerChain.OnException<DBConcurrencyException>()
                .Retry();
        }
    }

    public class MessageThatBombs
    {
    }

    public class MessageThatBombsHandler
    {
        public static int Throws = 3;
        public static int Attempts = 0;
        public static MessageThatBombs Successful;

        public void Consume(MessageThatBombs message)
        {
            Attempts++;

            if (Attempts <= Throws)
            {
                throw new DBConcurrencyException();
            }

            Successful = message;
        }
    }
}