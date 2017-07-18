using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionsStorage : ISubscriptionsStorage
    {
        private readonly ISubscriptionsRepository _repository;

        public SubscriptionsStorage(
            ISubscriptionsRepository repository)
        {
            _repository = repository;
        }

        public async Task PersistSubscriptions(Subscription[] subscriptions)
        {
            await _repository.PersistSubscriptions(subscriptions);
        }

        public Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            return _repository.LoadSubscriptions(subscriptionRole);
        }

        public Task RemoveSubscriptions(Subscription[] subscriptions)
        {
            return _repository.RemoveSubscriptions(subscriptions);
        }

        public Task ClearAll()
        {
            return Task.CompletedTask;
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            var all = await _repository.LoadSubscriptions(SubscriptionRole.Publishes);

            return all.Where(x => x.Matches(messageType)).ToArray();
        }

        public Task<Subscription[]> GetActiveSubscriptions()
        {
            return _repository.LoadSubscriptions(SubscriptionRole.Publishes);
        }
    }
}
