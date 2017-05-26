using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public class LocalSubscriptionRequirement : ISubscriptionRequirement
    {
        private readonly IList<Type> _messageTypes = new List<Type>();
        private readonly Uri _source;

        public LocalSubscriptionRequirement(Uri sourceProperty)
        {
            _source = sourceProperty;
        }

        public IEnumerable<Subscription> Determine(ChannelGraph graph)
        {
            if (_source == null) throw new InvalidOperationException("No Uri established for source.");

            var receiver = graph.IncomingChannelsFor(_source.Scheme).First();

            foreach (var messageType in _messageTypes)
            {
                yield return new Subscription(messageType)
                {
                    NodeName = graph.Name,
                    Receiver = receiver.Uri,
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
