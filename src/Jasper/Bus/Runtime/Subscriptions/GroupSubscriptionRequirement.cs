using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
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

        public IEnumerable<Subscription> Determine(ChannelGraph channels, SerializationGraph serialization)
        {
            if (_source == null) throw new InvalidOperationException("No Uri established for sender");

            if (_receiver == null) throw new InvalidOperationException("No Uri established for receiver.");



            foreach (var messageType in _messageTypes)
            {
                var contentTypes = serialization.ReaderFor(messageType.ToTypeAlias()).ContentTypes;

                yield return new Subscription(messageType)
                {
                    NodeName = channels.Name,
                    Receiver = _receiver,
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
