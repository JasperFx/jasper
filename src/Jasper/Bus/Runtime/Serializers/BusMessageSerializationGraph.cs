using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Model;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Bus.Runtime.Serializers
{
    public class BusMessageSerializationGraph : SerializationGraph
    {
        public static SerializationGraph Basic()
        {
            return new BusMessageSerializationGraph(new DefaultObjectPoolProvider(), new BusSettings(), new HandlerGraph(), new Forwarders(), new List<ISerializerFactory>(), new List<IMessageDeserializer>(), new List<IMessageSerializer>());
        }

        private readonly HandlerGraph _handlers;

        public BusMessageSerializationGraph(ObjectPoolProvider pooling, BusSettings busSettings, HandlerGraph handlers, Forwarders forwarders, IEnumerable<ISerializerFactory> serializers, IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
            : base(pooling, busSettings.MediaSelectionMode, busSettings.JsonSerialization, forwarders, serializers, readers, writers)
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
