using System.Collections.Generic;
using System.Linq;
using Bottles.Services;
using Bottles.Services.Messaging.Tracking;
using FubuCore;
using FubuTransportation.Events;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Serenity
{
    internal interface IMessageRecorder
    {
        IEnumerable<EnvelopeToken> ReceivedEnvelopes { get; }
        IEnumerable<object> ReceivedMessages { get; }
    }

    internal class MessageRecorder : IMessageRecorder, IListener, IListener<EnvelopeReceived>
    {
        private readonly IList<EnvelopeToken> _receivedEnvelopes = new List<EnvelopeToken>();

        public void Handle(EnvelopeReceived message)
        {
            EnvelopeToken envelope = message.Envelope;
            _receivedEnvelopes.Add(envelope);

            // Acknowledge that the message was received so it doesn't 
            // get stuck as Outstanding in the MessageHistory
            // Sometimes this happens in the wrong order so we need to 
            // wait for it to be in the pending list first.
            Wait.Until(() => MessageHistory.Outstanding().Any(x => x.Id == envelope.CorrelationId),
                millisecondPolling: 50, 
                timeoutInMilliseconds: 1000);
            var track = new MessageTrack
            {
                FullName = "{0}@{1}".ToFormat(envelope.CorrelationId, envelope.Destination),
                Id = envelope.CorrelationId,
                Status = MessageTrack.Received,
                Type = "OutstandingEnvelope"
            };
            Bottles.Services.Messaging.EventAggregator.SendMessage(track);
        }

        public IEnumerable<EnvelopeToken> ReceivedEnvelopes
        {
            get { return _receivedEnvelopes; }
        }

        public IEnumerable<object> ReceivedMessages
        {
            get { return _receivedEnvelopes.Select(x => x.Message); }
        }
    }
}