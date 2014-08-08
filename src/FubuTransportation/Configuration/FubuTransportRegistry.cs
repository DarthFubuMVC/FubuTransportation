﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bottles;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuMVC.Core.Configuration;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Diagnostics;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Registration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Routing;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.Sagas;
using FubuTransportation.ScheduledJob;
using FubuTransportation.Scheduling;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Configuration
{
    public class FubuTransportRegistry : IFubuRegistryExtension
    {
        private readonly IList<IHandlerSource> _sources = new List<IHandlerSource>();
        internal readonly PollingJobHandlerSource _pollingJobs = new PollingJobHandlerSource(); // leave it as internal
        internal readonly ScheduledJobGraph _scheduledJobs = new ScheduledJobGraph();
        private readonly IList<Action<ChannelGraph>> _channelAlterations = new List<Action<ChannelGraph>>();
        private readonly IList<Action<FubuRegistry>> _alterations = new List<Action<FubuRegistry>>();
        private readonly ConfigurationActionSet _localPolicies = new ConfigurationActionSet(ConfigurationType.Policy);
        private readonly ProvenanceChain _provenance;
        private string _name;

        public static FubuTransportRegistry For(Action<FubuTransportRegistry> configure)
        {
            var registry = new FubuTransportRegistry();
            configure(registry);

            return registry;
        }

        public static HandlerGraph HandlerGraphFor(Action<FubuTransportRegistry> configure)
        {
            var behaviors = BehaviorGraphFor(configure);

            return behaviors.Settings.Get<HandlerGraph>();
        }

        public static BehaviorGraph BehaviorGraphFor(Action<FubuTransportRegistry> configure)
        {
            var registry = new FubuRegistry();
            var transportRegistry = new FubuTransportRegistry();

            configure(transportRegistry);

            transportRegistry.As<IFubuRegistryExtension>()
                .Configure(registry);

            new FubuTransportationExtensions().Configure(registry);

            return BehaviorGraph.BuildFrom(registry);
        }

        /// <summary>
        /// Import configuration from an extension
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Import<T>() where T : IFubuTransportRegistryExtension, new()
        {
            new T().Configure(this);
        }

        /// <summary>
        /// Import configuration from an extentension with configured options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        public void Import<T>(Action<T> configuration) where T : IFubuTransportRegistryExtension, new()
        {
            var extension = new T();
            configuration(extension);

            extension.Configure(this);
        }

        /// <summary>
        /// Import configuration from an extension
        /// </summary>
        /// <param name="extension"></param>
        public void Import(IFubuTransportRegistryExtension extension)
        {
            extension.Configure(this);
        }

        public void SagaStorage<T>() where T : ISagaStorage, new()
        {
            AlterSettings<TransportSettings>(x => x.SagaStorageProviders.Add(new T()));
        }

        public static FubuTransportRegistry Empty()
        {
            return new FubuTransportRegistry();
        }

        protected FubuTransportRegistry()
        {
            _provenance = new ProvenanceChain(new Provenance[] {new FubuTransportRegistryProvenance(this)});

            _sources.Add(new DefaultHandlerSource());

            AlterSettings<ChannelGraph>(x => {
                if (x.Name.IsEmpty())
                {
                    x.Name = GetType().Name.Replace("TransportRegistry", "").Replace("Registry", "").ToLower();
                }
            });
        }

        public void AlterSettings<T>(Action<T> alteration) where T : new()
        {
            _alterations.Add(r => r.AlterSettings(alteration));
        }

        public string NodeName
        {
            set
            {
                _name = value;
                AlterSettings<ChannelGraph>(x => x.Name = value);
            }
            get
            {
                return _name;
            }
        }

        internal Action<ChannelGraph> channel
        {
            set { _channelAlterations.Add(value); }
        }


        private IEnumerable<IHandlerSource> allSources()
        {
            foreach (var handlerSource in _sources)
            {
                yield return handlerSource;
            }

            if (_pollingJobs.HasAny())
            {
                yield return _pollingJobs;
            }


            yield return _scheduledJobs;

        }

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            var graph = new HandlerGraph();
            var allCalls = allSources().SelectMany(x => x.FindCalls()).Distinct();
            graph.Add(allCalls);

            graph.ApplyPolicies(_localPolicies);

            registry.AlterSettings<HandlerGraph>(x => x.Import(graph));

            registry.AlterSettings<ChannelGraph>(channels => {
                _channelAlterations.Each(x => x(channels));
            });

            registry.Configure(behaviorGraph => {
                var channels = behaviorGraph.Settings.Get<ChannelGraph>();
                behaviorGraph.Services.Clear(typeof(ChannelGraph));
                behaviorGraph.Services.AddService(typeof(ChannelGraph), ObjectDef.ForValue(channels).AsSingleton());
            });

            _alterations.Each(x => x(registry));
        }

        /// <summary>
        ///   Finds the currently executing assembly.
        /// </summary>
        /// <returns></returns>
        public static Assembly FindTheCallingAssembly()
        {
            var trace = new StackTrace(false);

            var thisAssembly = Assembly.GetExecutingAssembly();
            var fubuCore = typeof (ITypeResolver).Assembly;
            var bottles = typeof (IPackageLoader).Assembly;
            var fubumvc = typeof (FubuRegistry).Assembly;


            Assembly callingAssembly = null;
            for (var i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly && assembly != fubuCore && assembly != bottles && assembly != fubumvc &&
                    !assembly.GetName().Name.StartsWith("System."))
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }

        public HandlersExpression Handlers
        {
            get { return new HandlersExpression(this); }
        }

        public class HandlersExpression
        {
            private readonly FubuTransportRegistry _parent;

            public HandlersExpression(FubuTransportRegistry parent)
            {
                _parent = parent;
            }

            public void Include(params Type[] types)
            {
                _parent._sources.Add(new ExplicitTypeHandlerSource(types));
            }

            public void Include<T>()
            {
                Include(typeof (T));
            }

            public void FindBy(Action<HandlerSource> configuration)
            {
                var source = new HandlerSource();
                configuration(source);

                _parent._sources.Add(source);
            }

            public void FindBy<T>() where T : IHandlerSource, new()
            {
                _parent._sources.Add(new T());
            }

            public void FindBy(IHandlerSource source)
            {
                _parent._sources.Add(source);
            }

            /// <summary>
            /// Completely remove the default handler finding
            /// logic.  This is probably only applicable to 
            /// retrofitting FubuTransportation to existing 
            /// systems with a very different nomenclature
            /// than the defaults
            /// </summary>
            public void DisableDefaultHandlerSource()
            {
                _parent._sources.RemoveAll(x => x is DefaultHandlerSource);
            }
        }

        public PollingJobExpression Polling
        {
            get { return new PollingJobExpression(this); }
        }

        public ScheduledJobExpression ScheduledJob
        {
            get { return new ScheduledJobExpression(_scheduledJobs); }
        }

        public void DefaultSerializer<T>() where T : IMessageSerializer, new()
        {
            channel = graph => graph.DefaultContentType = new T().ContentType;
        }

        public void DefaultContentType(string contentType)
        {
            channel = graph => graph.DefaultContentType = contentType;
        }

        /// <summary>
        /// Applies a Policy to the handler chains created by only this
        /// FubuTransportRegistry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoliciesExpression Local
        {
            get { return new PoliciesExpression(x => _localPolicies.Fill(_provenance, x)); }
        }

        /// <summary>
        /// Applies a Policy to all FubuTransportation Handler chains
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoliciesExpression Global
        {
            get
            {
                return
                    new PoliciesExpression(policy => { AlterSettings<HandlerPolicies>(x => x.AddGlobal(policy, this)); });
            }
        }


        /// <summary>
        ///   Configures the <see cref = "IServiceRegistry" /> to specify dependencies. 
        ///   This is an IoC-agnostic method of dependency configuration that will be consumed by the underlying implementation (e.g., StructureMap)
        /// </summary>
        public void Services(Action<ServiceRegistry> configure)
        {
            _alterations.Add(r => r.Services(configure));
        }


        public void Services<T>() where T : ServiceRegistry, new()
        {
            _alterations.Add(r => r.Services<T>());
        }

        /// <summary>
        /// Enable the in memory transport
        /// </summary>
        public void EnableInMemoryTransport(Uri replyUri = null)
        {
            AlterSettings<TransportSettings>(x => x.EnableInMemoryTransport = true);

            if (replyUri != null)
            {
                AlterSettings<MemoryTransportSettings>(x => x.ReplyUri = replyUri);
            }
        }
    }

    public class FubuTransportRegistry<T> : FubuTransportRegistry
    {
        protected FubuTransportRegistry()
        {
            AlterSettings<TransportSettings>(x => x.SettingTypes.Fill(typeof (T)));
            AlterSettings<ChannelGraph>(graph =>
            {
                if (FubuTransport.DefaultSettings == typeof(T))
                {
                    FubuTransport.DefaultChannelGraph = graph;
                }
            });
        }


        public new static FubuTransportRegistry<T> Empty()
        {
            return new FubuTransportRegistry<T>();
        }


        public ChannelExpression Channel(Expression<Func<T, Uri>> expression)
        {
            return new ChannelExpression(this, expression);
        }

        public class ChannelExpression
        {
            private readonly FubuTransportRegistry<T> _parent;
            private readonly Accessor _accessor;

            public ChannelExpression(FubuTransportRegistry<T> parent, Expression<Func<T, Uri>> expression)
            {
                _parent = parent;
                _accessor = ReflectionHelper.GetAccessor(expression);
            }

            private Action<ChannelNode> alter
            {
                set
                {
                    _parent.channel = graph => {
                        var node = graph.ChannelFor(_accessor);
                        value(node);
                    };
                }
            }

            /// <summary>
            /// Add an IEnvelopeModifier that will apply to only this channel
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ChannelExpression ModifyWith<T>() where T : IEnvelopeModifier, new()
            {
                return ModifyWith(new T());
            }

            /// <summary>
            /// Add an IEnvelopeModifier that will apply to only this channel
            /// </summary>
            /// <param name="modifier"></param>
            /// <returns></returns>
            public ChannelExpression ModifyWith(IEnvelopeModifier modifier)
            {
                alter = node => node.Modifiers.Add(modifier);

                return this;
            }

            public ChannelExpression DefaultSerializer<TSerializer>() where TSerializer : IMessageSerializer, new()
            {
                alter = node => node.DefaultSerializer = new TSerializer();
                return this;
            }

            public ChannelExpression DefaultContentType(string contentType)
            {
                alter = node => node.DefaultContentType = contentType;
                return this;
            }

            public ChannelExpression ReadIncoming(IScheduler scheduler = null)
            {
                alter = node => {
                    var defaultScheduler = node.Scheduler;
                    node.Incoming = true;
                    node.Scheduler = scheduler ?? defaultScheduler;
                };
                return this;
            }

            public ChannelExpression ReadIncoming(SchedulerMaker<T> schedulerMaker)
            {
                alter = node => {
                    node.Incoming = true;
                    node.SettingsRules.Add(schedulerMaker);
                };
                return this;
            }


            public ChannelExpression AcceptsMessagesInNamespaceContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(NamespaceRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression AcceptsMessagesInNamespace(string @namespace)
            {
                alter = node => node.Rules.Add(new NamespaceRule(@namespace));
                return this;
            }

            public ChannelExpression AcceptsMessagesInAssemblyContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(AssemblyRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression AcceptsMessagesInAssembly(string assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);

                alter = node => node.Rules.Add(new AssemblyRule(assembly));
                return this;
            }

            public ChannelExpression AcceptsMessage<TMessage>()
            {
                alter = node => node.Rules.Add(new SingleTypeRoutingRule<TMessage>());
                return this;
            }

            public ChannelExpression AcceptsMessage(Type messageType)
            {
                alter =
                    node => node.Rules.Add(typeof (SingleTypeRoutingRule<>).CloseAndBuildAs<IRoutingRule>(messageType));
                return this;
            }

            public ChannelExpression AcceptsMessages(Expression<Func<Type, bool>> filter)
            {
                alter = node => node.Rules.Add(new LambdaRoutingRule(filter));
                return this;
            }

            public ChannelExpression AcceptsMessagesMatchingRule<TRule>() where TRule : IRoutingRule, new()
            {
                alter = node => node.Rules.Add(new TRule());
                return this;
            }
        }

        public ByThreadScheduleMaker<T> ByThreads(Expression<Func<T, int>> property)
        {
            return new ByThreadScheduleMaker<T>(property);
        }

        public ByTaskScheduleMaker<T> ByTasks(Expression<Func<T, int>> property)
        {
            return new ByTaskScheduleMaker<T>(property);
        }

        public SubscriptionExpression SubscribeAt(Expression<Func<T, Uri>> receiving)
        {
            return new SubscriptionExpression(this, receiving);
        }

        public SubscriptionExpression SubscribeLocally()
        {
            return new SubscriptionExpression(this, null);
        }

        public class SubscriptionExpression
        {
            private readonly FubuTransportRegistry<T> _parent;
            private readonly Expression<Func<T, Uri>> _receiving;

            public SubscriptionExpression(FubuTransportRegistry<T> parent, Expression<Func<T, Uri>> receiving)
            {
                _parent = parent;
                _receiving = receiving;

                parent.Services(r => {
                    r.FillType(typeof(ISubscriptionRequirement), typeof(SubscriptionRequirements<T>));
                });
            }

            /// <summary>
            /// Specify the publishing source of the events you want to subscribe to
            /// </summary>
            /// <param name="sourceProperty"></param>
            /// <returns></returns>
            public TypeSubscriptionExpression ToSource(Expression<Func<T, Uri>> sourceProperty)
            {
                ISubscriptionRequirement<T> requirement = _receiving == null
                    ? (ISubscriptionRequirement<T>) new LocalSubscriptionRequirement<T>(sourceProperty)
                    : new GroupSubscriptionRequirement<T>(sourceProperty, _receiving);

                _parent.Services(x => x.AddService(requirement));

                return new TypeSubscriptionExpression(requirement);
            }

            public class TypeSubscriptionExpression
            {
                private readonly ISubscriptionRequirement<T> _requirement;

                public TypeSubscriptionExpression(ISubscriptionRequirement<T> requirement)
                {
                    _requirement = requirement;
                }

                public TypeSubscriptionExpression ToMessage<TMessage>()
                {
                    _requirement.AddType(typeof(TMessage));

                    return this;
                }

                public TypeSubscriptionExpression ToMessage(Type messageType)
                {
                    _requirement.AddType(messageType);
                    return this;
                }
            }
        }
    }

    public class NulloHandlerSource : IHandlerSource
    {
        public IEnumerable<HandlerCall> FindCalls()
        {
            yield break;
        }
    }
}