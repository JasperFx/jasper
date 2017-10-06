using System.Linq;
using Jasper.Bus.Model;

namespace Jasper.Diagnostics.Messages
{
    public static class DiagnosticsHandler
    {
        public static InitialData Receive(RequestInitialData message, HandlerGraph graph)
        {
            var chains = graph.Chains.OrderBy(c => c.TypeName).Select(ChainModel.For);
            return new InitialData(chains);
        }
    }
}
