using System;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionsStorage
    {
        Task PersistSubscriptions(Subscription[] subscriptions);
        Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole);
        Task RemoveSubscriptions(Subscription[] subscriptions);
        Task ClearAll();

        Task<Subscription[]> GetSubscribersFor(Type messageType);
        Task<Subscription[]> GetActiveSubscriptions();
    }
}
