using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Marten;

namespace Jasper.Persistence.Marten.Subscriptions
{
    public class MartenNodeDiscovery : INodeDiscovery
    {
        private readonly IDocumentStore _documentStore;

        public async Task<ServiceNode[]> FindAllKnown()
        {
            using (var session = _documentStore.QuerySession())
            {
                var nodes = await session.Query<ServiceNode>().ToListAsync();
                return nodes.ToArray();
            }
        }

        public ServiceNode LocalNode { get; private set; }
        public async Task UnregisterLocalNode()
        {
            using (var session = _documentStore.LightweightSession())
            {
                session.Delete(LocalNode);
                await session.SaveChangesAsync();
            }
        }

        public MartenNodeDiscovery(MartenSubscriptionSettings settings)
        {
            _documentStore = settings.Store;
        }

        public async Task Register(ServiceNode local)
        {
            LocalNode = local;
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(LocalNode);
                await session.SaveChangesAsync();
            }
        }

        public async Task<ServiceNode[]> FindPeers()
        {
            using (var session = _documentStore.LightweightSession())
            {
                var peers = await session.Query<ServiceNode>()
                    .Where(x => x.ServiceName == LocalNode.ServiceName)
                    .ToListAsync();

                return peers.ToArray();
            }
        }
    }
}
