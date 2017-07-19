using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Marten;

namespace JasperBus.Marten
{
    public class MartenNodeDiscovery : INodeDiscovery
    {
        private readonly IDocumentStore _documentStore;

        public TransportNode LocalNode { get; private set; }

        public MartenNodeDiscovery(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public Task Register(TransportNode local)
        {
            LocalNode = local;
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(LocalNode);
                return session.SaveChangesAsync();
            }
        }

        public async Task<TransportNode[]> FindPeers()
        {
            using (var session = _documentStore.LightweightSession())
            {
                var peers = await session.Query<TransportNode>()
                    .Where(x => x.NodeName == LocalNode.NodeName && x.Id != LocalNode.Id)
                    .ToListAsync();

                return peers.ToArray();
            }
        }
    }
}
