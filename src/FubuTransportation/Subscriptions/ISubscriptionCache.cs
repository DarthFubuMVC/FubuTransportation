﻿using System;
using System.Collections.Generic;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionCache
    {
        IEnumerable<ChannelNode> FindDestinationChannels(Envelope envelope);

        Uri ReplyUriFor(ChannelNode destination);

        /// <summary>
        /// Called internally
        /// </summary>
        /// <param name="subscriptions"></param>
        void LoadSubscriptions(IEnumerable<Subscription> subscriptions);

        IEnumerable<Subscription> ActiveSubscriptions { get; }

        string NodeName { get; }
    }
}