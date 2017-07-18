using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        Task Register(ChannelGraph graph);
        Task<TransportNode[]> FindPeers();
        TransportNode LocalNode { get; set; }
    }

    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private readonly string _machineName;

        public InMemoryNodeDiscovery(EnvironmentSettings envSettings)
        {
            _machineName = envSettings.MachineName;
        }

        public Task Register(ChannelGraph graph)
        {
            LocalNode = new TransportNode(graph, _machineName);
            return Task.CompletedTask;
        }

        public Task<TransportNode[]> FindPeers()
        {
            return Task.FromResult(new TransportNode[0]);
        }

        public TransportNode LocalNode { get; set; }
    }
}
