using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        Task Register(TransportNode local);
        Task<TransportNode[]> FindPeers();
        TransportNode LocalNode { get; }
    }

    public class InMemoryNodeDiscovery : INodeDiscovery
    {
        private readonly string _machineName;

        public InMemoryNodeDiscovery(BusSettings envSettings)
        {
            _machineName = envSettings.MachineName;
        }

        public Task Register(TransportNode local)
        {
            LocalNode = local;
            return Task.CompletedTask;
        }

        public Task<TransportNode[]> FindPeers()
        {
            return Task.FromResult(new TransportNode[0]);
        }

        public TransportNode LocalNode { get; private set; }
    }
}
