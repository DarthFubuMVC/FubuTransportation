﻿using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionPersistence
    {
        IEnumerable<Subscription> LoadSubscriptions(string name, SubscriptionRole role);
        void Persist(IEnumerable<Subscription> subscriptions);
        void Persist(Subscription subscription);

        IEnumerable<TransportNode> NodesForGroup(string name);
        void Persist(params TransportNode[] nodes);

        IEnumerable<TransportNode> AllNodes();
        IEnumerable<Subscription> AllSubscriptions();
    }
}