using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Subscriptions;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionRepositoryTester
    {
        private readonly ISubscriptionsRepository _repository;

        public SubscriptionRepositoryTester()
        {
            _repository = new InMemorySubscriptionsRepository();
        }

        [Fact]
        public async Task store_subscriptions()
        {
            var subs = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(role: SubscriptionRole.Publishes),
                Subs.ExistingSubscription(role: SubscriptionRole.Publishes)
            };

            await _repository.PersistSubscriptions(subs);

            (await _repository.LoadSubscriptions(SubscriptionRole.Subscribes))
                .ShouldHaveTheSameElementsAs(subs.Where(x => x.Role == SubscriptionRole.Subscribes));

            (await _repository.LoadSubscriptions(SubscriptionRole.Publishes))
                .ShouldHaveTheSameElementsAs(subs.Where(x => x.Role == SubscriptionRole.Publishes));
        }

        [Fact]
        public async Task remove_subscriptions()
        {
            var subs = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            await _repository.PersistSubscriptions(subs);

            await _repository.RemoveSubscriptions(new [] { subs.Last() });

            var loadedSubs = await _repository.LoadSubscriptions(SubscriptionRole.Subscribes);
            loadedSubs.ShouldHaveTheSameElementsAs(subs.Take(2));
        }
    }
}
