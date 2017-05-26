using System;
using System.Collections.Generic;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public class GroupSubscriptionRequirement : ISubscriptionRequirement
    {
        private readonly Uri _source;
        private readonly Uri _receiver;
        private readonly IList<Type> _messageTypes = new List<Type>();

        public GroupSubscriptionRequirement(Uri sourceProperty, Uri receiverProperty)
        {
            _source = sourceProperty;
            _receiver = receiverProperty;
        }

        public IEnumerable<Subscription> Determine(ChannelGraph graph)
        {
            if (_source == null) throw new InvalidOperationException("No Uri established for sender");

            if (_receiver == null) throw new InvalidOperationException("No Uri established for receiver.");

            foreach (var messageType in _messageTypes)
            {
                yield return new Subscription(messageType)
                {
                    NodeName = graph.Name,
                    Receiver = _receiver,
                    Source = _source
                };
            }
        }

        public void AddType(Type type)
        {
            _messageTypes.Add(type);
        }
    }
}
