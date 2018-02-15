using System.Threading.Tasks;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private readonly string _machineName;

        public InMemoryNodeDiscovery(MessagingSettings envSettings)
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

        public Task<ServiceNode[]> FindAllKnown()
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