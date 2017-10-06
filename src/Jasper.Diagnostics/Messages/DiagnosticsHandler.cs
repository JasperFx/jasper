using System.Linq;
using Jasper.Bus.Model;

namespace Jasper.Diagnostics.Messages
{
    public class DiagnosticsHandler
    {
        private readonly HandlerGraph _graph;

        public DiagnosticsHandler(HandlerGraph graph)
        {
            _graph = graph;
        }

        public InitialData Receive(RequestInitialData message)
        {
            var chains = _graph.Chains.OrderBy(c => c.TypeName).Select(ChainModel.For);
            return new InitialData(chains);
        }
    }
}
