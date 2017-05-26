using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace JasperBus.Runtime.Subscriptions
{
    public interface ISubscriptionsRepository : IDisposable
    {
        void PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole);
        void RemoveSubscriptions(IEnumerable<Subscription> subscriptions);
    }

    public class InMemorySubscriptionsRepository : ISubscriptionsRepository
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly List<Action<IEnumerable<Subscription>>> _handlers = new List<Action<IEnumerable<Subscription>>>();

        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            subscriptions.Each(sub => _subscriptions.Remove(sub));
        }

        public void Dispose()
        {
        }

        public void PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            subscriptions.Where(x => !_subscriptions.Contains(x)).Each(x => _subscriptions.Add(x));
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            return _subscriptions.Where(x => x.Role == subscriptionRole);
        }
    }
}
