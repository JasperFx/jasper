using System.Linq;
using JasperBus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Subscriptions
{
    public class SubscriptionRepositoryTester
    {
        private readonly ISubscriptionsRepository _repository;

        public SubscriptionRepositoryTester()
        {
            _repository = new InMemorySubscriptionsRepository();
        }

        [Fact]
        public void store_subscriptions()
        {
            var subs = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(role: SubscriptionRole.Publishes),
                Subs.ExistingSubscription(role: SubscriptionRole.Publishes)
            };

            _repository.PersistSubscriptions(subs);

            _repository.LoadSubscriptions(SubscriptionRole.Subscribes)
                .ShouldHaveTheSameElementsAs(subs.Where(x => x.Role == SubscriptionRole.Subscribes));

            _repository.LoadSubscriptions(SubscriptionRole.Publishes)
                .ShouldHaveTheSameElementsAs(subs.Where(x => x.Role == SubscriptionRole.Publishes));
        }

        [Fact]
        public void remove_subscriptions()
        {
            var subs = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            _repository.PersistSubscriptions(subs);

            _repository.RemoveSubscriptions(new [] { subs.Last() });

            var loadedSubs = _repository.LoadSubscriptions(SubscriptionRole.Subscribes);
            loadedSubs.ShouldHaveTheSameElementsAs(subs.Take(2));
        }
    }
}
