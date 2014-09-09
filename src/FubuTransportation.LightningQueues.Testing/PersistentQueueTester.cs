﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Transactions;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using LightningQueues;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class PersistentQueueTester
    {
        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void creates_queues_when_started()
        {
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>(), new LightningQueueSettings()))
            {
                queues.ClearAll();
                queues.Start(new LightningUri[]
                {
                    new LightningUri("lq.tcp://localhost:2424/some_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/other_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/third_queue"), 
                });

                queues.ManagerFor(2424, true)
                    .Queues.OrderBy(x => x).ShouldHaveTheSameElementsAs(LightningQueuesTransport.DelayedQueueName, LightningQueuesTransport.ErrorQueueName, "other_queue", "some_queue", "third_queue");
            }
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void default_settings_match_lightning_queues_default_configuration()
        {
            var defaultConfiguration = new QueueManagerConfiguration();
            //could probably do this without constructing the queue manager, but probably safer down the road this way
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>(), new LightningQueueSettings()))
            {
                queues.ClearAll();
                queues.Start(new[] {new LightningUri("lq.tcp://localhost:2424/some_queue")});
                var queueManager = queues.ManagerFor(2424, true);
                var actual = queueManager.Configuration;
                actual.EnableOutgoingMessageHistory.ShouldEqual(defaultConfiguration.EnableOutgoingMessageHistory);
                actual.EnableProcessedMessageHistory.ShouldEqual(defaultConfiguration.EnableProcessedMessageHistory);
                actual.NumberOfMessagesToKeepInOutgoingHistory.ShouldEqual(defaultConfiguration.NumberOfMessagesToKeepInOutgoingHistory);
                actual.NumberOfMessagesToKeepInProcessedHistory.ShouldEqual(defaultConfiguration.NumberOfMessagesToKeepInProcessedHistory);
                actual.NumberOfReceivedMessageIdsToKeep.ShouldEqual(defaultConfiguration.NumberOfReceivedMessageIdsToKeep);
                actual.OldestMessageInOutgoingHistory.ShouldEqual(defaultConfiguration.OldestMessageInOutgoingHistory);
                actual.OldestMessageInProcessedHistory.ShouldEqual(defaultConfiguration.OldestMessageInProcessedHistory);
            }
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void settings_that_are_changed_are_also_changed_in_queue_manager()
        {
            var settings = new LightningQueueSettings
            {
                EnableOutgoingMessageHistory = false,
                EnableProcessedMessageHistory = false,
                NumberOfMessagesToKeepInOutgoingHistory = 1,
                NumberOfMessagesToKeepInProcessedHistory = 2,
                NumberOfReceivedMessageIdsToKeep = 3,
                OldestMessageInOutgoingHistory = TimeSpan.FromSeconds(1),
                OldestMessageInProcessedHistory = TimeSpan.FromSeconds(1),
            };
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>(), settings))
            {
                queues.ClearAll();
                queues.Start(new[] { new LightningUri("lq.tcp://localhost:2424/some_queue") });
                var queueManager = queues.ManagerFor(2424, true);
                var actual = queueManager.Configuration;
                actual.EnableOutgoingMessageHistory.ShouldEqual(settings.EnableOutgoingMessageHistory);
                actual.EnableProcessedMessageHistory.ShouldEqual(settings.EnableProcessedMessageHistory);
                actual.NumberOfMessagesToKeepInOutgoingHistory.ShouldEqual(settings.NumberOfMessagesToKeepInOutgoingHistory);
                actual.NumberOfMessagesToKeepInProcessedHistory.ShouldEqual(settings.NumberOfMessagesToKeepInProcessedHistory);
                actual.NumberOfReceivedMessageIdsToKeep.ShouldEqual(settings.NumberOfReceivedMessageIdsToKeep);
                actual.OldestMessageInOutgoingHistory.ShouldEqual(settings.OldestMessageInOutgoingHistory);
                actual.OldestMessageInProcessedHistory.ShouldEqual(settings.OldestMessageInProcessedHistory);
            }
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void recovers_delayed_messages_when_started()
        {
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>(), new LightningQueueSettings()))
            {
                queues.ClearAll();
                queues.Start(new []{ new LightningUri("lq.tcp://localhost:2425/the_queue") });

                var envelope = new Envelope();
                envelope.Data = new byte[0];
                envelope.ExecutionTime = DateTime.UtcNow;
                var delayedMessage = new MessagePayload
                {
                    Data = envelope.Data,
                    Headers = envelope.Headers.ToNameValues()
                };

                using (var scope = new TransactionScope())
                {
                    queues.ManagerFor(2425, true)
                        .EnqueueDirectlyTo(LightningQueuesTransport.DelayedQueueName, null, delayedMessage); 
                    scope.Complete();
                }
            }

            var cache = new DelayedMessageCache<MessageId>();
            using (var queues = new PersistentQueues(new RecordingLogger(), cache, new LightningQueueSettings()))
            {
                queues.Start(new []{ new LightningUri("lq.tcp://localhost:2425/the_queue") });

                cache.AllMessagesBefore(DateTime.UtcNow.AddSeconds(1)).ShouldNotBeEmpty();
            }
        }
    }
}