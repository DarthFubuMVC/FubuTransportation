﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Dates;
using FubuTransportation.Events;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Subscriptions;

namespace FubuTransportation
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEventAggregator _events;
        private readonly IChainInvoker _invoker;
        private readonly ISystemTime _systemTime;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public ServiceBus(IEnvelopeSender sender, IEventAggregator events, IChainInvoker invoker, ISystemTime systemTime, ISubscriptionRepository subscriptionRepository)
        {
            _sender = sender;
            _events = events;
            _invoker = invoker;
            _systemTime = systemTime;
            _subscriptionRepository = subscriptionRepository;
        }

        // The destination override is tested as part of the monitoring integration
        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            options = options ?? new RequestOptions();

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = typeof(TResponse).Name
            };

            if (options.Destination != null)
            {
                envelope.Destination = options.Destination;
            }

            var listener = new ReplyListener<TResponse>(_events, envelope.CorrelationId, options.Timeout);
            _events.AddListener(listener);

            _sender.Send(envelope);

            return listener.Completion;
        }

        public void Send<T>(T message)
        {
            _sender.Send(new Envelope {Message = message});
        }

        public void Send<T>(Uri destination, T message)
        {
            _sender.Send(new Envelope {Message = message, Destination = destination});
        }

        public void Consume<T>(T message)
        {
            _invoker.InvokeNow(message);
        }

        public void DelaySend<T>(T message, DateTime time)
        {
            _sender.Send(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
            });
        }

        public void DelaySend<T>(T message, TimeSpan delay)
        {
            DelaySend(message, _systemTime.UtcNow().Add(delay));
        }

        public Task SendAndWait<T>(T message)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true
            };

            var listener = new ReplyListener<Acknowledgement>(_events, envelope.CorrelationId, 10.Minutes());
            _events.AddListener(listener);

            _sender.Send(envelope);

            return listener.Completion;
        }

        public void RemoveSubscriptionsForThisNode()
        {
            var subscriptions = _subscriptionRepository.LoadSubscriptions(SubscriptionRole.Subscribes);
            if (!subscriptions.Any())
                return;

            subscriptions = _subscriptionRepository.RemoveLocalSubscriptions();
            subscriptions.GroupBy(x => x.Source)
                .Each(x =>
                {
                    var envelope = new Envelope
                    {
                        Destination = x.Key,
                        Message = new SubscriptionsRemoved { Receiver = x.First().Receiver }
                    };
                    _sender.Send(envelope);
                });
        }
    }
}