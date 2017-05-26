using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        void Register(ChannelGraph graph);
        IEnumerable<TransportNode> FindPeers();
        TransportNode LocalNode { get; set; }
    }

    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private string _machineName;

        public InMemoryNodeDiscovery(EnvironmentSettings envSettings)
        {
            _machineName = envSettings.MachineName;
        }

        public void Register(ChannelGraph graph)
        {
            LocalNode = new TransportNode(graph, _machineName);
        }

        public IEnumerable<TransportNode> FindPeers()
        {
            return Enumerable.Empty<TransportNode>();
        }

        public TransportNode LocalNode { get; set; }
    }
}
