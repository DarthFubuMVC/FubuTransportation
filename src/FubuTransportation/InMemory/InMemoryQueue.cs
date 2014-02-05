﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using FubuCore;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using System.Linq;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.InMemory
{
    public class InMemoryQueue : IDisposable
    {
        private readonly Uri _uri;
        private BlockingCollection<byte[]> _queue = InitializeQueue();
        private readonly BinaryFormatter _formatter;
        private bool _disposed;

        public InMemoryQueue(Uri uri)
        {
            _uri = uri;
            _formatter = new BinaryFormatter();
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public void Enqueue(EnvelopeToken envelope)
        {
            using (var stream = new MemoryStream())
            {
                _formatter.Serialize(stream, envelope);

                stream.Position = 0;
                var bytes = stream.ReadAllBytes();

                _queue.Add(bytes);
            }
        }

        public IEnumerable<Envelope> Peek()
        {
            return _queue.ToArray().Select(x => _formatter.Deserialize(new MemoryStream(x)).As<Envelope>());
        }

        public void Clear()
        {
            var oldQueue = _queue;
            _queue = InitializeQueue();
            oldQueue.CompleteAdding();
        }

        public void Dispose()
        {
            _disposed = true;
            _queue.CompleteAdding();
        }

        public void Receive(IReceiver receiver)
        {
            while (!_disposed)
            {
                foreach (var data in _queue.GetConsumingEnumerable())
                {
                    using (var stream = new MemoryStream(data))
                    {
                        var token = _formatter.Deserialize(stream).As<EnvelopeToken>();

                        var callback = new InMemoryCallback(this, token);

                        receiver.Receive(token.Data, token.Headers, callback);
                    }
                }
            }
        }

        private static BlockingCollection<byte[]> InitializeQueue()
        {
            return new BlockingCollection<byte[]>(new ConcurrentBag<byte[]>());
        }
    }

    public class InMemoryCallback : IMessageCallback
    {
        private readonly InMemoryQueue _parent;
        private readonly EnvelopeToken _token;

        public InMemoryCallback(InMemoryQueue parent, EnvelopeToken token)
        {
            _parent = parent;
            _token = token;
        }

        public void MarkSuccessful()
        {
            // nothing
        }

        public void MarkFailed()
        {
            Debug.WriteLine("Message was marked as failed!");
        }

        public void MoveToDelayedUntil(DateTime time)
        {
            //TODO leverage delayed message cache?
            InMemoryQueueManager.AddToDelayedQueue(_token);
        }

        public void MoveToErrors(ErrorReport report)
        {
            throw new NotImplementedException();
        }

        public void Requeue()
        {
            throw new NotImplementedException();
        }
    }
}