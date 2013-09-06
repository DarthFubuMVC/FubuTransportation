﻿using System.Collections.Generic;
using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class EnvelopeModifierTester
    {
        [Test]
        public void abstract_modifier_is_actually_useful()
        {
            FakeEnvelopeModifier.Modified.Clear();

            var modifier = new FakeEnvelopeModifier();

            modifier.Modify(new Envelope{Message = new Message()});
            modifier.Modify(new Envelope{Message = new OneMessage()});
            modifier.Modify(new Envelope{Message = new TwoMessage()});
            modifier.Modify(new Envelope{Message = new GreenFoo()});

            FakeEnvelopeModifier.Modified.Select(x => x.GetType())
                .ShouldHaveTheSameElementsAs(typeof(Message), typeof(OneMessage), typeof(TwoMessage));
        }
    }

    public class FakeEnvelopeModifier : EnvelopeModifier<Message>
    {
        public static readonly IList<Message> Modified = new List<Message>();

        public override void Modify(Envelope envelope, Message target)
        {
            Modified.Add(target);
        }
    }
}