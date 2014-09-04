﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class MonitoredNodeGroup : IDisposable
    {
        private readonly Cache<string, MonitoredNode> _nodes = new Cache<string, MonitoredNode>();
        private readonly InMemorySubscriptionPersistence _persistence = new InMemorySubscriptionPersistence();
        private readonly IList<Action<MonitoredNode>> _configurations = new List<Action<MonitoredNode>>();
        private readonly PersistentTaskMessageListener _listener = new PersistentTaskMessageListener();

        public void Add(string nodeId, Uri incoming)
        {
            var node = new MonitoredNode(nodeId, incoming, _listener);
            _nodes[nodeId] = node;
        }

        public void AddTask(Uri subject, string initialNode, IEnumerable<string> preferredNodes)
        {
            _configurations.Add(node => node.AddTask(subject, preferredNodes));

            if (!initialNode.EqualsIgnoreCase("none"))
            {
                _nodes[initialNode].AddInitialTask(subject);
            }
            
        }

        public bool MonitoringEnabled { get; set; }

        public IEnumerable<PersistentTaskMessage> LoggedEvents()
        {
            return _listener.LoggedEvents();
        } 

        public MonitoredNode NodeFor(string id)
        {
            return _nodes[id];
        }

        public void Startup()
        {
            _nodes.Each(node => {
                _configurations.Each(x => x(node));
                node.Startup(MonitoringEnabled, _persistence);
            });
        }

        public void Dispose()
        {
            _nodes.Each(x => x.SafeDispose());
        }

        public void SetTaskState(Uri subject, string node, string state)
        {
            var task = _nodes[node].TaskFor(subject);
            task.SetState(state);
        }

        public IEnumerable<TaskState> AssignedTasks()
        {
            return _nodes.SelectMany(x => x.AssignedTasks()).Where(x => x.Task.Scheme != "scheduled");
        }

        public IEnumerable<TaskState> PersistedTasks()
        {
            return
                _persistence.AllNodes()
                    .SelectMany(node => {
                        return node.OwnedTasks.Select(x => new TaskState { Node = node.Id, Task = x });
                    }).Where(x => x.Task.Scheme != "scheduled");
        }

        public void WaitForAllHealthChecks()
        {
            var tasks = _nodes.Select(x => {
                return Task.Delay(MonitoredNode.Random.Next(25, 200)).ContinueWith(t => {
                    return x.WaitForHealthCheck();
                }).Unwrap();
            });

            Task.WaitAll(tasks.ToArray(), 15.Seconds());

        }

        public void ShutdownNode(string node)
        {
            _nodes[node].Shutdown();
            _nodes.Remove(node);
        }

        public IEnumerable<TransportNode> GetPersistedNodes()
        {
            return _persistence.NodesForGroup("Monitoring");
        }

        public void WaitForHealthChecksOn(string node)
        {
            _nodes[node].WaitForHealthCheck().Wait(15.Seconds());
        }
    }

    public class TaskState
    {
        public Uri Task { get; set; }
        public string Node { get; set; }
    }
}