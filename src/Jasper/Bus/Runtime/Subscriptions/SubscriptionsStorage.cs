using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Bus.Runtime.Subscriptions
{
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

        public async Task PersistSubscriptions(Subscription[] subscriptions)
        {
            await _repository.PersistSubscriptions(subscriptions);
            _cache.Store(subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));
        }

        public async Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            var subscriptions = await _repository.LoadSubscriptions(subscriptionRole);
            if (subscriptionRole == SubscriptionRole.Publishes)
            {
                _cache.Store(subscriptions);
            }

            return subscriptions;
        }

        public async Task RemoveSubscriptions(Subscription[] subscriptions)
        {
            await _repository.RemoveSubscriptions(subscriptions);
            subscriptions.Each(x => _cache.Remove(x));
        }

        public Task ClearAll()
        {
            _cache.ClearAll();

            return Task.CompletedTask;
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            var subscriptions = await GetActiveSubscriptions();
            return subscriptions.Where(x => x.Matches(messageType)).ToArray();
        }

        public Task<Subscription[]> GetActiveSubscriptions() => Task.FromResult(_cache.ActiveSubscriptions);
    }
}
