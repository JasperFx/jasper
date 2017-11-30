using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Diagnostics.Messages
{
    public static class DiagnosticsHandler
    {
        public static async Task<DiagnosticData> Receive(RequestDiagnosticData message, HandlerGraph graph, ISubscriptionsRepository subscriptionsRepository, JasperRuntime runtime)
        {
            var storedSubs = await subscriptionsRepository.GetSubscriptions();
            var chains = graph.Chains.OrderBy(c => c.TypeName).Select(ChainModel.For);
            var pubs = runtime.Capabilities.Published;
            var declaredSubs = runtime.Capabilities.Subscriptions;
            return new DiagnosticData(chains, storedSubs, pubs,declaredSubs);
        }
    }
}
