using System.Collections.Generic;
using Jasper.Conneg;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace JasperHttp
{
    public class HttpSerializationGraph : SerializationGraph<IMessageDeserializer, IMessageSerializer>
    {
        public HttpSerializationGraph(IEnumerable<ISerializerFactory<IMessageDeserializer, IMessageSerializer>> serializers, IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers) : base(serializers, readers, writers)
        {
        }
    }
}
