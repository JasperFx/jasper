using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class WriteJson : IWriterRule
    {
        public bool Applies(RouteChain chain)
        {
            return true;
        }

        public IEnumerable<Frame> DetermineWriters(RouteChain chain)
        {
            chain.WriterType = typeof(NewtonsoftJsonWriter<>)
                .MakeGenericType(chain.ResourceType);

            yield return new UseWriter(chain);
        }
    }
}