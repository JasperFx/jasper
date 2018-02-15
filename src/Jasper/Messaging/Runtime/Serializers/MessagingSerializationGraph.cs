using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Conneg;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Messaging.Runtime.Serializers
{
    public class MessagingSerializationGraph : SerializationGraph
    {
        public static MessagingSerializationGraph Basic()
        {
            return new MessagingSerializationGraph(new DefaultObjectPoolProvider(), new MessagingSettings(), new HandlerGraph(), new Forwarders(), new List<ISerializerFactory>(), new List<IMessageDeserializer>(), new List<IMessageSerializer>());
        }

        private readonly HandlerGraph _handlers;

        public MessagingSerializationGraph(ObjectPoolProvider pooling, MessagingSettings messagingSettings, HandlerGraph handlers, Forwarders forwarders, IEnumerable<ISerializerFactory> serializers, IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
            : base(pooling, messagingSettings.MediaSelectionMode, messagingSettings.JsonSerialization, forwarders, serializers, readers, writers)
        {
            _handlers = handlers;

            // Work around here to seed this type in the serialization
            RegisterType(typeof(Acknowledgement));
        }

        protected override IEnumerable<Type> determineChainCandidates(string messageType)
        {
            return _handlers.Chains.Where(x => x.MessageType.ToMessageAlias() == messageType)
                .Select(x => x.MessageType);
        }
    }
}
