﻿using System;
using System.Collections.Specialized;
using System.Transactions;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Queues;
using Rhino.Queues.Model;
using FubuTestingSupport;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class when_creating_an_envelope_from_a_rhino_queue_message
    {
        private Message message;
        private Envelope theEnvelope;

        [SetUp]
        public void SetUp()
        {
            message = new Message
            {
                Data = new byte[] {1, 2, 3, 4},
                Headers = new NameValueCollection(),
                Id = new MessageId {MessageIdentifier = Guid.NewGuid()}
            };

            message.Headers["a"] = "1";
            message.Headers["b"] = "2";
            message.Headers["c"] = "3";

            theEnvelope = RhinoQueuesChannel.ToEnvelope(message);
        }

        [Test]
        public void should_copy_the_data()
        {
            theEnvelope.Data.ShouldEqual(message.Data);
        }

        [Test]
        public void should_copy_the_headers()
        {
            theEnvelope.Headers["a"].ShouldEqual("1");
            theEnvelope.Headers["b"].ShouldEqual("2");
            theEnvelope.Headers["c"].ShouldEqual("3");
        }

        [Test]
        public void should_copy_the_id()
        {
            theEnvelope.Headers["Id"].ShouldEqual(message.Id.MessageIdentifier.ToString());
        }
    }
}