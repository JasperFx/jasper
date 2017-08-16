using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class LocalSubscriptionRequirement : ISubscriptionRequirement
    {
        private readonly IList<Type> _messageTypes = new List<Type>();
        private readonly Uri _source;

        public LocalSubscriptionRequirement(Uri sourceProperty)
        {
            _source = sourceProperty;
        }

        public IEnumerable<Subscription> Determine(ChannelGraph channels, SerializationGraph serialization)
        {
            if (_source == null) throw new InvalidOperationException("No Uri established for source.");

            var receiver = channels.IncomingChannelsFor(_source.Scheme).First();

            foreach (var messageType in _messageTypes)
            {
                var contentTypes = serialization.ReaderFor(messageType.ToTypeAlias()).ContentTypes;

                yield return new Subscription(messageType, receiver.Uri)
                {
                    Publisher = channels.Name,
                    Source = _source,
                    Accepts = contentTypes.Join(",")
                };
            }
        }

        public void AddType(Type type)
        {
            _messageTypes.Add(type);
        }
    }
}
