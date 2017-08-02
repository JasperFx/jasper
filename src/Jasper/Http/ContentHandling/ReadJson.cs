using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class ReadJson : IReaderRule
    {
        public bool Applies(RouteChain chain)
        {
            return true;
        }

        public IEnumerable<Frame> DetermineReaders(RouteChain chain)
        {
            chain.ReaderType = typeof(NewtonsoftJsonReader<>)
                .MakeGenericType(chain.InputType);

            yield return new UseReader(chain);
        }
    }
}