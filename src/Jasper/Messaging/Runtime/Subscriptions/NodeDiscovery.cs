using System.Threading.Tasks;

namespace Jasper.Messaging.Runtime.Subscriptions
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
