using System.Linq;
using JasperBus.Model;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics.Messages
{
    public class DiagnosticsListener : IListener<RequestInitialData>
    {
        private readonly IDiagnosticsClient _client;
        private readonly HandlerGraph _graph;

        public DiagnosticsListener(
            IDiagnosticsClient client,
            HandlerGraph graph)
        {
            _client = client;
            _graph = graph;
        }

        public void Receive(RequestInitialData message)
        {
            var chains = _graph.Chains.OrderBy(c => c.TypeName).Select(ChainModel.For);
            _client.Send(new InitialData(chains));
        }
    }
}
