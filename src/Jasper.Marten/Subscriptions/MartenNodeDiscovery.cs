using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Subscriptions;
using Marten;

namespace Jasper.Marten.Subscriptions
{
    public class MartenNodeDiscovery : INodeDiscovery
    {
        private readonly IDocumentStore _documentStore;

        public TransportNode LocalNode { get; private set; }

        public MartenNodeDiscovery(MartenSubscriptionSettings settings)
        {
            _documentStore = settings.Store;
        }

        public async Task Register(TransportNode local)
        {
            LocalNode = local;
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(LocalNode);
                await session.SaveChangesAsync();
            }
        }

        public async Task<TransportNode[]> FindPeers()
        {
            using (var session = _documentStore.LightweightSession())
            {
                var peers = await session.Query<TransportNode>()
                    .Where(x => x.ServiceName == LocalNode.ServiceName)
                    .ToListAsync();

                return peers.ToArray();
            }
        }
    }
}
