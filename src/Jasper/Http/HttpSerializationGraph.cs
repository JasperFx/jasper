using System.Collections.Generic;
using Jasper.Conneg;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Http
{
    public class HttpSerializationGraph : SerializationGraph
    {
        public HttpSerializationGraph(HttpSettings settings, ObjectPoolProvider pooling, Forwarders forwarders, IEnumerable<ISerializerFactory> serializers, IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
            : base(pooling, settings.JsonSerialization, serializers, readers, writers)
        {
        }
    }
}
