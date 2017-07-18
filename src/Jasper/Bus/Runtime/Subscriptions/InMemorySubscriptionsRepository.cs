using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class InMemorySubscriptionsRepository : ISubscriptionsRepository
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                _subscriptions.Remove(subscription);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            _subscriptions.Fill(subscriptions);
            return Task.CompletedTask;
        }

        public Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            var matching = _subscriptions.Where(x => x.Role == subscriptionRole).ToArray();
            return Task.FromResult(matching);
        }
    }

    public interface ISubscriptionsRepository : IDisposable
    {
        Task PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole);
        Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions);
    }
}
