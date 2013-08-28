﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Runtime;
using System.Linq;

namespace FubuTransportation.InMemory
{

    public static class InMemoryQueueManager
    {
        public static readonly Uri DelayedUri = "memory://localhost/delayed".ToUri();

        private static readonly Cache<Uri, InMemoryQueue> _queues = new Cache<Uri,InMemoryQueue>(x => new InMemoryQueue(x));
        private static readonly IList<EnvelopeToken> _delayed = new List<EnvelopeToken>(); 
        private static readonly ReaderWriterLockSlim _delayedLock = new ReaderWriterLockSlim();
    
        public static void ClearAll()
        {
            _delayedLock.Write(() => {
                _delayed.Clear();
            });

            
            _queues.Each(x => x.SafeDispose());
            _queues.ClearAll();
        }

        public static void AddToDelayedQueue(EnvelopeToken envelope)
        {
            _delayedLock.Write(() => {
                _delayed.Add(envelope);
            });
        }

        public static IEnumerable<EnvelopeToken> DequeueDelayedEnvelopes(DateTime currentTime)
        {
            var delayed = _delayedLock.Read(() => {
                return _delayed.Where(x => new Envelope(x.Headers).ExecutionTime.Value <= currentTime).ToArray();
            });

            var list = new List<EnvelopeToken>();

            foreach (EnvelopeToken token in delayed)
            {
                _delayedLock.Write(() => {
                    try
                    {
                        _delayed.Remove(token);

                        var envelope = new Envelope(token.Headers);
                        _queues[envelope.ReceivedAt].Enqueue(token);

                        list.Add(token);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });

            }

            return list;
        } 

        public static InMemoryQueue QueueFor(Uri uri)
        {
            return _queues[uri];
        }

        public static IEnumerable<EnvelopeToken> DelayedEnvelopes()
        {
            return _delayedLock.Read(() => {
                return _delayed.ToArray();
            });
        }
    }
}