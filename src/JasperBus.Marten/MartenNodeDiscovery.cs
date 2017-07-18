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
        private readonly string _machineName;

        public TransportNode LocalNode { get; set; }

        public MartenNodeDiscovery(IDocumentStore documentStore, EnvironmentSettings envSettings)
        {
            _documentStore = documentStore;
            _machineName = envSettings.MachineName;
        }

        public Task Register(ChannelGraph graph)
        {
            LocalNode = new TransportNode(graph, _machineName);
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
