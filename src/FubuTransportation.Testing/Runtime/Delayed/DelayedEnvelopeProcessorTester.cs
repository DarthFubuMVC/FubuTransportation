﻿using System;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using FubuCore;

namespace FubuTransportation.Testing.Runtime.Delayed
{
    [TestFixture]
    public class DelayedEnvelopeProcessorTester : InteractionContext<DelayedEnvelopeProcessor>
    {
        [Test]
        public void dequeue_from_all_the_transports()
        {
            var transports = Services.CreateMockArrayFor<ITransport>(4);

            LocalSystemTime = DateTime.Today.AddHours(5);

            ClassUnderTest.Execute();

            transports.Each(transport => {
                transport.AssertWasCalled(x => x.ReplayDelayed(UtcSystemTime));
            });
        }

        [Test]
        public void dequeue_a_single_transport_should_log_all_the_requeued_envelopes()
        {
            var logger = new RecordingLogger();
            Services.Inject<ILogger>(logger);

            var envelopes = new EnvelopeToken[] {new EnvelopeToken(), new EnvelopeToken(), new EnvelopeToken()};
            LocalSystemTime = DateTime.Today.AddHours(5);
            var theTransport = MockFor<ITransport>();

            theTransport.Stub(x => x.ReplayDelayed(UtcSystemTime))
                        .Return(envelopes);

            ClassUnderTest.DequeuFromTransport(theTransport, UtcSystemTime);

            envelopes.Each(env => {
                logger.InfoMessages.ShouldContain(new DelayedEnvelopeAddedBackToQueue{Envelope = env});
            });
        }
    }

}