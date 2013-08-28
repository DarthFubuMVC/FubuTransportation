﻿using System;
using System.Collections.Generic;
using FubuTransportation.Configuration;
namespace FubuTransportation.Runtime
{
    public interface ITransport : IDisposable
    {
        void OpenChannels(ChannelGraph graph);
        string Protocol { get; }

        /// <summary>
        /// This is mostly for the cases where we have
        /// to register new channels at runtime through
        /// dynamic subscriptions or reply channels
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        IChannel BuildChannel(ChannelNode node);

        IEnumerable<EnvelopeToken> ReplayDelayed(DateTime currentTime);
    }
}