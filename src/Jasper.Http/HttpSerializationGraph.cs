using System.Collections.Generic;
using Jasper.Http.ContentHandling;
using Jasper.Serialization;

namespace Jasper.Http
{
    public class HttpSerializationGraph : SerializationGraph<IRequestReader, IResponseWriter>
    {
        public HttpSerializationGraph(IEnumerable<ISerializerFactory<IRequestReader, IResponseWriter>> serializers,
            IEnumerable<IRequestReader> readers, IEnumerable<IResponseWriter> writers) : base(serializers, readers,
            writers)
        {
        }
    }
}
