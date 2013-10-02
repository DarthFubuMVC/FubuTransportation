﻿using FubuTransportation.Configuration;

namespace FubuTransportation.Scheduling
{
    public class ShutdownChannelNodeVisitor : IChannelNodeVisitor
    {
        public void Visit(ChannelNode node)
        {
            node.Channel.Dispose();
            node.Scheduler.Dispose();
        }
    }
}