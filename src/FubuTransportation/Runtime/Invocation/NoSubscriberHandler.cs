﻿using System.ComponentModel;
using FubuCore.Descriptions;

namespace FubuTransportation.Runtime.Invocation
{
    [Description("Policies for handling messages that have no registered handlers")]
    public class NoSubscriberHandler : SimpleEnvelopeHandler
    {
        public override bool Matches(Envelope envelope)
        {
            return true;
        }

        public override void Execute(Envelope envelope, ContinuationContext context)
        {
            context.Outgoing.SendFailureAcknowledgement(envelope, "No subscriber");
            envelope.Callback.MarkSuccessful();
            // TODO -- do more here.  There's a GH issue for this.
        }
    }
}