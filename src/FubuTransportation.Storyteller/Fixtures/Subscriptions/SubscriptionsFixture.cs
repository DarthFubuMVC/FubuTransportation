﻿using System.Linq;
using System.Reflection;
using Bottles.Services.Messaging.Tracking;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Subscriptions;
using HtmlTags;
using StoryTeller;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures.Subscriptions
{
    public class SubscriptionsFixture : Fixture
    {
        private readonly Cache<string, RunningNode> _nodes = new Cache<string, RunningNode>();
        private RunningNode _node;

        public SubscriptionsFixture()
        {
            AddSelectionValues("FubuTransportRegistries",
                Assembly.GetExecutingAssembly()
                    .ExportedTypes.Where(x => x.IsConcreteTypeOf<FubuTransportRegistry>())
                    .Select(x => x.Name)
                    .ToArray());
        }

        public override void SetUp(ITestContext context)
        {
            RunningNode.Subscriptions.ClearAll();
            MessageHistory.ClearAll();
            InMemoryQueueManager.ClearAll();
            FubuTransport.ApplyMessageHistoryWatching = true;
        }

        public override void TearDown()
        {
            _nodes.Each(x => x.Dispose());
        }

        [FormatAs("Load a node {Key} from {Registry} with reply Uri {ReplyUri}")]
        public void LoadNode(string Key, [SelectionValues("FubuTransportRegistries")] string Registry, string ReplyUri)
        {
            MessageHistory.WaitForWorkToFinish(() => {
                var node = new RunningNode(Registry, ReplyUri.ToUri());
                node.Start();

                _nodes[Key] = node;

                Context.Trace(new CodeTag(Key, node.Contents));
            });
        }

        [FormatAs("For node {Key}")]
        public void ForNode(string Key)
        {
            _node = _nodes[Key];
        }

        public IGrammar TheActiveSubscriptionsAre()
        {
            return VerifySetOf(() => _node.LoadedSubscriptions())
                .Titled("The active subscriptions for publishing are")
                .MatchOn(x => x.NodeName, x => x.MessageType, x => x.Source, x => x.Receiver);
        }

        public IGrammar ThePersistedSubscriptionsAre()
        {
            return VerifySetOf(() => _node.PersistedSubscriptions())
                .Titled("The persisted subscriptions for publishing are")
                .MatchOn(x => x.NodeName, x => x.MessageType, x => x.Source, x => x.Receiver);
        }

        public IGrammar TheLocalSubscriptionsAre()
        {
            return VerifySetOf(() => _node.PersistedSubscriptions(SubscriptionRole.Subscribes))
                .Titled("The persisted roles for subscribing are")
                .MatchOn(x => x.NodeName, x => x.MessageType, x => x.Source, x => x.Receiver);
        }

        public IGrammar ThePersistedTransportNodesAre()
        {
            return VerifySetOf(() => _node.PersistedNodes().Select(x => new TransportNodeItem(x)))
                .Titled("The persisted transport nodes are")
                .MatchOn(x => x.NodeName, x => x.Address);
        }

        [FormatAs("Node {Key} removes local subscriptions")]
        public void NodeRemovesLocalSubscritpions(string Key)
        {
            _nodes[Key].RemoveSubscriptions();
        }
    }

    public class TransportNodeItem
    {
        public TransportNodeItem(TransportNode node)
        {
            NodeName = node.NodeName;
            Address = node.Addresses.FirstOrDefault(x => x.Scheme == InMemoryChannel.Protocol).ToString();
        }

        public string NodeName { get; set; }
        public string Address { get; set; }
    }


    public class CodeTag : HtmlTag
    {
        public CodeTag(string name, string code) : base("h4")
        {
            Text(name);
            Next = new HtmlTag("pre", tag => {
                tag.Style("border", "thin solid black");
                tag.Style("padding", "5px");
                tag.Text(code);
                tag.Next = new HtmlTag("hr");
            });
        }
    }
}