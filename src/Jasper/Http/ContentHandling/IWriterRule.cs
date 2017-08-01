using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public interface IWriterRule
    {
        bool Applies(RouteChain chain);
        IEnumerable<Frame> DetermineWriters(RouteChain chain);
    }
}