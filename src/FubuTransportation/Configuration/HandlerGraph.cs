﻿using System;
using System.Collections;
using System.Collections.Generic;
using FubuCore;
using System.Linq;
using FubuMVC.Core.Configuration;
using FubuMVC.Core.Registration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.Configuration
{
    [ApplicationLevel]
    public class HandlerGraph : IEnumerable<HandlerChain>
    {
        private readonly IDictionary<Type, HandlerChain> _chains = new Dictionary<Type, HandlerChain>(); 
        private readonly IList<HandlerCall> _stagedCalls = new List<HandlerCall>();

        public void Add(IEnumerable<HandlerCall> calls)
        {
            calls.Each(Add);
        }

        public void Add(HandlerCall call)
        {
            _stagedCalls.Add(call);

            var inputType = call.InputType();
            if (inputType.IsConcrete())
            {
                if (_chains.ContainsKey(inputType))
                {
                    call.AddClone(_chains[inputType]);
                }
                else
                {
                    var chain = new HandlerChain();
                    _chains.Add(inputType, chain);
                    call.AddClone(chain);
                }
            }
        }

        public void ApplyGeneralizedHandlers()
        {
            _stagedCalls.Each(call => {
                var matching = _chains.Values.Where(x => call.CouldHandleOtherMessageType(x.InputType()));
                matching.Each(call.AddClone);
            });
        }

        public HandlerChain ChainFor(Type inputType)
        {
            return _chains[inputType];
        }

        public IEnumerator<HandlerChain> GetEnumerator()
        {
            return _chains.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Import(HandlerGraph other)
        {
            _stagedCalls.Fill(other._stagedCalls);

            other._chains.Values.Each(chain => {
                var inputType = chain.InputType();
                if (_chains.ContainsKey(inputType))
                {
                    var original = _chains[inputType];

                    chain.ToArray().Each(original.AddToEnd);
                }
                else
                {
                    _chains.Add(inputType, chain);
                }
            });
        }

        public void ApplyPolicies(ConfigurationActionSet actions)
        {
            var graph = new BehaviorGraph();
            _chains.Values.Each(graph.AddChain);

            actions.RunActions(graph);
        }
    }
}