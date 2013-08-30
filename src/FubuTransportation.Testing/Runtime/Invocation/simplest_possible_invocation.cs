﻿using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using System.Linq;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class simplest_possible_invocation : InvocationContext
    {
        [Test]
        public void single_message_for_single_handler_one()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new OneMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<OneHandler>(theMessage);
        }

        [Test]
        public void single_message_for_single_handler_two()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new TwoMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<TwoHandler>(theMessage);


        }

        [Test]
        public void single_message_for_single_handler_three()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new ThreeMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<ThreeHandler>(theMessage);
        }

        [Test]
        public void single_message_for_single_handler_four()
        {
            handler<OneHandler, TwoHandler, ThreeHandler, FourHandler>();

            var theMessage = new FourMessage();
            sendMessage(theMessage);

            TestMessageRecorder.AllProcessed.Single()
                               .ShouldMatch<FourHandler>(theMessage);
        }
    }
}