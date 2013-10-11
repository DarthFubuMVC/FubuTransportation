﻿using System;
using FubuTestingSupport;
using FubuTransportation.Diagnostics;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing.Diagnostics
{
    [TestFixture]
    public class when_creating_a_message_record_from_envelope_token
    {
        private EnvelopeToken theToken;
        private MessageRecord theRecord;

        [SetUp]
        public void SetUp()
        {
            theToken = ObjectMother.EnvelopeWithMessage().ToToken();
            theToken.ParentId = Guid.NewGuid().ToString();
            theToken.Headers["A"] = "1";
            theToken.Headers["B"] = "2";

            theRecord = new MessageRecord(theToken);
        }

        [Test]
        public void capture_the_correlation_id()
        {
            theRecord.Id.ShouldEqual(theToken.CorrelationId);
        }

        [Test]
        public void capture_the_parent_id()
        {
            theRecord.ParentId.ShouldEqual(theToken.ParentId);
        }

        [Test]
        public void capture_the_message_type()
        {
            theRecord.Type.ShouldEqual(theToken.Message.GetType().FullName);
        }

        [Test]
        public void capture_the_headers()
        {
            theRecord.Headers.ShouldContain("A=1");
            theRecord.Headers.ShouldContain("B=2");
        }
    }
}