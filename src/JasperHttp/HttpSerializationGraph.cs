using System.Collections.Generic;
using Jasper.Conneg;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace JasperHttp
{
    public class HttpSerializationGraph : SerializationGraph
    {
        public HttpSerializationGraph(JsonSerializerSettings settings, ObjectPoolProvider pooling, Forwarders forwarders,
            IEnumerable<ISerializerFactory> serializers, IEnumerable<IMessageDeserializer> readers,
            IEnumerable<IMessageSerializer> writers)
            : base(pooling, settings, serializers, readers, writers)
        {
        }
    }
}
