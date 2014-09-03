﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Subscriptions
{
    public class TransportNode
    {
        private readonly IList<Uri> _addresses = new List<Uri>();

        public TransportNode()
        {
        }

        public TransportNode(ChannelGraph graph)
        {
            NodeName = graph.Name;


            Addresses = graph.ReplyUriList().ToArray();

            if (!Addresses.Any())
            {
                throw new InvalidOperationException("At least one reply channel is required");
            }
            
            MachineName = Environment.MachineName;
            Id = graph.NodeId;
        }

        public string Id { get; set; }
        public string MachineName { get; set; }
        public string NodeName { get; set; }

        private readonly IList<Uri> _ownedTasks = new List<Uri>();

        public void AddOwnership(Uri subject)
        {
            _ownedTasks.Fill(subject);
        }

        public void AddOwnership(IEnumerable<Uri> subjects)
        {
            _ownedTasks.Fill(subjects);
        }

        public void RemoveOwnership(Uri subject)
        {
            _ownedTasks.Remove(subject);
        }

        public Uri[] OwnedTasks
        {
            get
            {
                return _ownedTasks.ToArray();
            }
            set
            {
                _ownedTasks.Clear();
                if (value != null) _ownedTasks.AddRange(value);
            }
        }

        public Uri[] Addresses
        {
            get
            {
                return _addresses.ToArray();
            }
            set
            {
                _addresses.Clear();
                if (value != null) _addresses.AddRange(value);
            }
        }

        protected bool Equals(TransportNode other)
        {
            return _addresses.OrderBy(x => x.ToString()).SequenceEqual(other._addresses.OrderBy(x => x.ToString()));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransportNode) obj);
        }

        public override int GetHashCode()
        {
            return (_addresses != null ? _addresses.GetHashCode() : 0);
        }
    }
}