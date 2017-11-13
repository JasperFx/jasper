using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        Task Register(ServiceNode local);
        Task<ServiceNode[]> FindPeers();
        ServiceNode LocalNode { get; }

        Task UnregisterLocalNode();
    }

    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private readonly string _machineName;

        public InMemoryNodeDiscovery(BusSettings envSettings)
        {
            _machineName = envSettings.MachineName;
        }

        public Task Register(ServiceNode local)
        {
            LocalNode = local;
            return Task.CompletedTask;
        }

        public Task<ServiceNode[]> FindPeers()
        {
            return Task.FromResult(new ServiceNode[0]);
        }

        public ServiceNode LocalNode { get; private set; }
        public Task UnregisterLocalNode()
        {
            LocalNode = null;
            return Task.CompletedTask;
        }
    }
}
