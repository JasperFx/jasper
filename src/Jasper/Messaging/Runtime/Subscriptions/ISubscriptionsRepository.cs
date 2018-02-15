using System;
using System.Threading.Tasks;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public interface ISubscriptionsRepository : IDisposable
    {
        Task RemoveCapabilities(string serviceName);
        Task PersistCapabilities(ServiceCapabilities capabilities);
        Task<ServiceCapabilities> CapabilitiesFor(string serviceName);
        Task<ServiceCapabilities[]> AllCapabilities();

        Task<Subscription[]> GetSubscribersFor(Type messageType);
        Task<Subscription[]> GetSubscriptions();
    }


}
