﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.ModelBinding;
using FubuCore;
using FubuTransportation.Subscriptions;
using HtmlTags;
using StoryTeller;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class MonitoringFixture : Fixture
    {
        private MonitoredNodeGroup _nodes;

        public MonitoringFixture()
        {
            Title = "Health Monitoring, Failover, and Task Assignment";

            this["Context"] = Embed<MonitoringSetupFixture>("If the nodes and tasks are");


        }

        public override void SetUp(ITestContext context)
        {
            _nodes = new MonitoredNodeGroup();
            context.Store(_nodes);
        }

        public override void TearDown()
        {
            var messages = _nodes.LoggedEvents().ToArray();
            var table = new TableTag();
            table.AddHeaderRow(_ => {
                _.Header("Node");
                _.Header("Subject");
                _.Header("Type");
                _.Header("Message");
            });

            messages.Each(message => {
                table.AddBodyRow(_ => {
                    _.Cell(message.NodeId);
                    _.Cell(message.Subject.ToString());
                    _.Cell(message.GetType().Name);
                    _.Cell(message.ToString());
                });
            });

            Context.Trace(table);

            _nodes.Dispose();
        }

        [ExposeAsTable("If the task state is")]
        public void TaskStateIs(
            Uri Task, 
            string Node, 
            [SelectionValues(MonitoredNode.HealthyAndFunctional, MonitoredNode.TimesOutOnStartupOrHealthCheck, MonitoredNode.ThrowsExceptionOnStartupOrHealthCheck, MonitoredNode.IsInactive)]string State)
        {
            _nodes.SetTaskState(Task, Node, State);
        }

        [FormatAs("After the health checks run on all nodes")]
        public void AfterTheHealthChecksRunOnAllNodes()
        {
            _nodes.WaitForAllHealthChecks();
        }

        [FormatAs("After the health checks run on node {node}")]
        public void AfterTheHealthChecksRunOnNode(string node)
        {
            _nodes.WaitForHealthChecksOn(node);
        }

        [FormatAs("Node {Node} drops offline")]
        public void NodeDropsOffline(string Node)
        {
            _nodes.ShutdownNode(Node);
        }

        public IGrammar TheTaskAssignmentsShouldBe()
        {
            return VerifySetOf(() => _nodes.AssignedTasks())
                .Titled("The task assignments should be")
                .MatchOn(x => x.Task, x => x.Node);
        }

        public IGrammar ThePersistedAssignmentsShouldBe()
        {
            return VerifySetOf(() => _nodes.PersistedTasks())
                .Titled("The persisted task assignments should be")
                .MatchOn(x => x.Task, x => x.Node);
        }

        public IGrammar ThePersistedNodesShouldBe()
        {
            return VerifySetOf<TransportNode>(() => _nodes.GetPersistedNodes())
                .Titled("The persisted nodes should be")
                .MatchOn(x => x.Id, x => x.ControlChannel);
        }
    }

    
}