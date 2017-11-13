using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface INodeDiscovery
    {
        Task Register(ServiceNode local);
        Task<ServiceNode[]> FindPeers();
        Task<ServiceNode[]> FindAllKnown();
        ServiceNode LocalNode { get; }

        Task UnregisterLocalNode();


    }
}
