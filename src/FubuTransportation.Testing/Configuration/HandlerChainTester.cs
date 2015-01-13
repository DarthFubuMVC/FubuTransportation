﻿using System.Threading.Tasks;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerChainTester
    {

        [Test]
        public void the_default_number_of_maximum_attempts_is_1()
        {
            new HandlerChain().MaximumAttempts.ShouldEqual(1);
        }

        [Test]
        public void HandlerChain_cannot_be_marked_as_partial_only_because_it_knocks_out_diagnostic_tracing()
        {
            new HandlerChain().IsPartialOnly.ShouldBeFalse();
        }


        [Test]
        public void is_async_negative()
        {
            var chain = new HandlerChain();
            chain.IsAsync.ShouldBeFalse();
        
            chain.AddToEnd(HandlerCall.For<GreenHandler>(x => x.Handle(new Message1())));

            chain.IsAsync.ShouldBeFalse();
        
        }

        [Test]
        public void is_async_positive()
        {
            var chain = new HandlerChain();
            chain.IsAsync.ShouldBeFalse();

            chain.AddToEnd(HandlerCall.For<GreenHandler>(x => x.Handle(new Message1())));
            chain.AddToEnd(HandlerCall.For<AsyncHandler>(x => x.Go(null)));

            chain.IsAsync.ShouldBeTrue();


        }

        public class AsyncHandler
        {
            public Task Go(Message message)
            {
                return null;
            }

            public Task<string> Other(Message message)
            {
                return null;
            }
        }
    }
}