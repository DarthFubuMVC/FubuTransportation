﻿using System;
using Bottles;
using FubuCore.Logging;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Events;
using FubuTransportation.InMemory;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Cascading;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.TestSupport;
using NUnit.Framework;
using System.Linq;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportServiceRegistry_spec
    {
        private void registeredTypeIs<TService, TImplementation>()
        {
            registeredTypeIs(typeof(TService), typeof(TImplementation));
        }

        private void registeredTypeIs(Type service, Type implementation)
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            BehaviorGraph.BuildFrom(registry)
                .Services.DefaultServiceFor(service)
                .Type.ShouldEqual(implementation);
        }

        [Test]
        public void service_bus_is_registered()
        {
            registeredTypeIs<IServiceBus, ServiceBus>();
        }

        [Test]
        public void subscriptions_is_registered_as_singleton()
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var @default = BehaviorGraph.BuildFrom(registry).Services
                                        .DefaultServiceFor<ISubscriptions>();

            @default.ShouldNotBeNull();
            @default.Type.ShouldEqual(typeof (Subscriptions));
            @default.IsSingleton.ShouldBeTrue();



        }

        [Test]
        public void envelope_sender_is_registered()
        {
            registeredTypeIs<IEnvelopeSender, EnvelopeSender>();
        }

        [Test]
        public void in_memory_transport_is_registered()
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            BehaviorGraph.BuildFrom(registry).Services
                         .ServicesFor<ITransport>().Single(x => x.Type == typeof (InMemoryTransport))
                         .ShouldNotBeNull();
        }

        [Test]
        public void event_aggregation_listener_is_registered()
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            BehaviorGraph.BuildFrom(registry).Services
                         .ServicesFor<ILogListener>()
                         .Any(x => x.Type == typeof(EventAggregationListener))
                         .ShouldBeTrue();
        }

        [Test]
        public void event_aggregator_is_registered_as_a_singleton_by_default()
        {
            FubuTransport.UseSynchronousLogging = false;

            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var @default = BehaviorGraph.BuildFrom(registry).Services.DefaultServiceFor<IEventAggregator>();

            @default.Type.ShouldEqual(typeof (EventAggregator));
            @default.IsSingleton.ShouldBeTrue();

        }

        [Test]
        public void use_synchronous_event_aggregator_if_FubuTransport_UseSynchronousLogging()
        {
            FubuTransport.UseSynchronousLogging = true;

            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var @default = BehaviorGraph.BuildFrom(registry).Services.DefaultServiceFor<IEventAggregator>();

            @default.Type.ShouldEqual(typeof(SynchronousEventAggregator));
            @default.IsSingleton.ShouldBeTrue();
        }

        [Test]
        public void saga_state_cache_is_registered_as_a_singleton()
        {
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var @default = BehaviorGraph.BuildFrom(registry).Services.DefaultServiceFor<ISagaStateCacheFactory>();

            @default.Type.ShouldEqual(typeof(SagaStateCacheFactory));
            @default.IsSingleton.ShouldBeTrue();


        }

        [Test]
        public void message_watcher_is_registered_as_listener_if_FubuTransport_messagewatching_is_applied()
        {
            FubuTransport.ApplyMessageHistoryWatching = true;
            
            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var serviceGraph = BehaviorGraph.BuildFrom(registry).Services;
            serviceGraph.ServicesFor<IListener>().Any(x => x.Type == typeof (MessageWatcher)).ShouldBeTrue();
        }

        [Test]
        public void message_watcher_is_NOT_registered_as_listener_if_FubuTransport_messagewatching_is_NOT_applied()
        {
            FubuTransport.ApplyMessageHistoryWatching = false;

            var registry = new FubuRegistry();
            registry.Services<FubuTransportServiceRegistry>();
            var serviceGraph = BehaviorGraph.BuildFrom(registry).Services;
            serviceGraph.ServicesFor<IListener>().Any(x => x.Type == typeof(MessageWatcher)).ShouldBeFalse();
        }

        [Test]
        public void EnvelopeSerializer_is_registered()
        {
            registeredTypeIs<IEnvelopeSerializer, EnvelopeSerializer>();
        }

        [Test]
        public void MessageInvoker_is_registered()
        {
            registeredTypeIs<IChainInvoker, ChainInvoker>();
        }

        [Test]
        public void XmlMessageSerializer_is_registered()
        {
            registeredTypeIs<IMessageSerializer, XmlMessageSerializer>();
        }

        [Test]
        public void TransportActivator_is_registered()
        {
            registeredTypeIs<IActivator, TransportActivator>();
        }


        [Test]
        public void handler_pipeline_is_registered()
        {
            registeredTypeIs<IHandlerPipeline, HandlerPipeline>();
        }

        [Test]
        public void message_executor_is_registered()
        {
            registeredTypeIs<IMessageExecutor, MessageExecutor>();
        }

        [Test]
        public void outgoing_messages_is_registered()
        {
            registeredTypeIs<IOutgoingSender, OutgoingSender>();
        }
    }
}