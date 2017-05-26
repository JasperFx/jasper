using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Subscriptions;
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

        public void Register(ChannelGraph graph)
        {
            LocalNode = new TransportNode(graph, _machineName);
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(LocalNode);
                session.SaveChanges();
            }
        }

        public IEnumerable<TransportNode> FindPeers()
        {
            using (var session = _documentStore.LightweightSession())
            {
                return session.Query<TransportNode>()
                    .Where(x => x.NodeName == LocalNode.NodeName && x.Id != LocalNode.Id)
                    .ToList();
            }
        }
    }
}
