using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public interface IReaderRule
    {
        bool Applies(RouteChain chain);
        IEnumerable<Frame> DetermineReaders(RouteChain chain);
    }
}