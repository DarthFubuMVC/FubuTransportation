﻿using System;
using System.Collections;
using System.Collections.Generic;
using FubuCore;
using System.Linq;

namespace FubuTransportation.Configuration
{
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
    }
}