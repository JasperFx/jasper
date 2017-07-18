using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionsRepository : IDisposable
    {
        Task PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole);
        Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions);

        Task<Subscription[]> GetSubscribersFor(Type messageType);
    }

    public static class SubscriptionStorageExtensions
    {
        public static Task<Subscription[]> GetActiveSubscriptions(this ISubscriptionsRepository storage)
        {
            return storage.LoadSubscriptions(SubscriptionRole.Publishes);
        }
    }
}
