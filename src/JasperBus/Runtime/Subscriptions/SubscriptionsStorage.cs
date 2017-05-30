using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace JasperBus.Runtime.Subscriptions
{
    public interface ISubscriptionsStorage
    {
        void PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole);
        void RemoveSubscriptions(IEnumerable<Subscription> subscriptions);
        void ClearAll();
        IEnumerable<Uri> GetSubscribersFor(Type messageType);
        IEnumerable<Subscription> ActiveSubscriptions { get; }
    }

    public class SubscriptionsStorage : ISubscriptionsStorage
    {
        private readonly ISubscriptionsCache _cache;
        private readonly ISubscriptionsRepository _repository;

        public SubscriptionsStorage(
            ISubscriptionsCache cache,
            ISubscriptionsRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public void PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            _repository.PersistSubscriptions(subscriptions);
            _cache.Store(subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            var subscriptions = _repository.LoadSubscriptions(subscriptionRole).ToList();
            if (subscriptionRole == SubscriptionRole.Publishes)
            {
                _cache.Store(subscriptions);
            }
            return subscriptions;
        }

        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            _repository.RemoveSubscriptions(subscriptions);
            subscriptions.Each(x => _cache.Remove(x));
        }

        public void ClearAll()
        {
            _cache.ClearAll();
        }

        public IEnumerable<Uri> GetSubscribersFor(Type messageType)
        {
            return ActiveSubscriptions
                .Where(x => x.Matches(messageType))
                .Select(x => x.Receiver);
        }

        public IEnumerable<Subscription> ActiveSubscriptions => _cache.ActiveSubscriptions;
    }
}
