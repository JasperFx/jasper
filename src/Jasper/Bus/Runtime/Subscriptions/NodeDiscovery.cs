using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        void Register(ChannelGraph graph);
        IEnumerable<TransportNode> FindPeers();
        TransportNode LocalNode { get; set; }
    }

    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private readonly string _machineName;

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
