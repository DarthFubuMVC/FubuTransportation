﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    // Tested through Storyteller tests
    public class SubscriptionActivator : IActivator
    {
        private readonly ISubscriptionRepository _repository;
        private readonly IEnvelopeSender _sender;
        private readonly ISubscriptionCache _cache;
        private readonly IEnumerable<ISubscriptionRequirement> _requirements;
        private readonly ChannelGraph _graph;

        public SubscriptionActivator(ISubscriptionRepository repository, IEnvelopeSender sender, ISubscriptionCache cache, IEnumerable<ISubscriptionRequirement> requirements, ChannelGraph graph)
        {
            _repository = repository;
            _sender = sender;
            _cache = cache;
            _requirements = requirements;
            _graph = graph;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            log.Trace("Determining subscriptions for node " + _cache.NodeName);

            // assuming that there are no automaticly persistent tasks
            // upon startup
            _repository.Persist(new TransportNode(_graph));

            var requirements = determineStaticRequirements(log);


            if (requirements.Any())
            {
                log.Trace("Found static subscription requirements:");
                requirements.Each(x => log.Trace(x.ToString()));
            }
            else
            {
                log.Trace("No static subscriptions found from registry");
            }

            _repository.PersistSubscriptions(requirements);

            var subscriptions = _repository.LoadSubscriptions(SubscriptionRole.Publishes);
            _cache.LoadSubscriptions(subscriptions);

            sendSubscriptions();
        }

        private Subscription[] determineStaticRequirements(IPackageLog log)
        {
            var requirements = _requirements.SelectMany(x => x.DetermineRequirements()).ToArray();
            traceLoadedRequirements(log, requirements);
            return requirements;
        }

        private void sendSubscriptions()
        {
            _repository.LoadSubscriptions(SubscriptionRole.Subscribes)
                .GroupBy(x => x.Source)
                .Each(group => sendSubscriptionsToSource(@group.Key, @group));
        }

        private static void traceLoadedRequirements(IPackageLog log, Subscription[] requirements)
        {
            log.Trace("Found subscription requirements:");
            requirements.Each(x => log.Trace(x.ToString()));
        }

        private void sendSubscriptionsToSource(Uri destination, IEnumerable<Subscription> subscriptions)
        {
            var envelope = new Envelope
            {
                Message = new SubscriptionRequested
                {
                    Subscriptions = subscriptions.ToArray()
                },
                Destination = destination
            };

            _sender.Send(envelope);
        }
    }
}