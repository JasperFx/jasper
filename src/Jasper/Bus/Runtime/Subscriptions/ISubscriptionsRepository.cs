using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionsRepository : IDisposable
    {
        Task PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions);

        Task<Subscription[]> GetSubscribersFor(Type messageType);
        Task<Subscription[]> GetSubscriptions();
        Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions);
    }


}
