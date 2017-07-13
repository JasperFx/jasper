using System.Linq;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionsStorageTester
    {
        private readonly ISubscriptionsStorage _storage;
        private readonly ISubscriptionsCache _cache;
        private readonly ISubscriptionsRepository _repository;

        private readonly Subscription[] _subscriptions = {
                Subs.NewSubscription(),
                Subs.NewSubscription(),
                Subs.NewSubscription(role:SubscriptionRole.Publishes),
                Subs.NewSubscription(role:SubscriptionRole.Publishes),
                Subs.NewSubscription(role:SubscriptionRole.Publishes),
                Subs.NewSubscription(),
                Subs.NewSubscription()
            };

        public SubscriptionsStorageTester()
        {
            _cache = new SubscriptionsCache();
            _repository = new InMemorySubscriptionsRepository();
            _storage = new SubscriptionsStorage(_cache, _repository);
        }

        [Fact]
        public void persists_subscriptions()
        {
            _storage.PersistSubscriptions(_subscriptions);

            _repository.LoadSubscriptions(SubscriptionRole.Publishes)
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));

            _repository.LoadSubscriptions(SubscriptionRole.Subscribes)
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Subscribes));

            _cache.ActiveSubscriptions
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));
        }

        [Fact]
        public void removes_subscriptions()
        {
            _storage.PersistSubscriptions(_subscriptions);

            _storage.RemoveSubscriptions(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));

            _repository.LoadSubscriptions(SubscriptionRole.Publishes)
                .ShouldHaveCount(0);

            _cache.ActiveSubscriptions.ShouldHaveCount(0);
        }

        [Fact]
        public void adds_missing_subscriptions_to_cache()
        {
            _storage.PersistSubscriptions(_subscriptions);

            _repository.PersistSubscriptions(new []
            {
                Subs.NewSubscription(role:SubscriptionRole.Publishes)
            });

            var expectedPublish = _subscriptions.Count(x => x.Role == SubscriptionRole.Publishes) + 1;

            _storage.LoadSubscriptions(SubscriptionRole.Publishes)
                .ShouldHaveCount(expectedPublish);

            _cache.ActiveSubscriptions.ShouldHaveCount(expectedPublish);
        }

        [Fact]
        public void subscribers_for_message()
        {
            var sub1 = Subs.NewSubscription(role: SubscriptionRole.Publishes);
            sub1.MessageType = typeof(PingMessage).FullName;
            sub1.Receiver = "memory://PingReceiver".ToUri();

            var sub2 = Subs.NewSubscription(role: SubscriptionRole.Publishes);
            sub2.MessageType = typeof(PongMessage).FullName;
            sub2.Receiver = "memory://PongReceiver".ToUri();

            var sub3 = Subs.NewSubscription(role: SubscriptionRole.Publishes);
            sub3.MessageType = typeof(PingMessage).FullName;
            sub3.Receiver = "memory://PingReceiver2".ToUri();

            _storage.PersistSubscriptions(new[] {sub1, sub2, sub3});

            var pingSubscribers = _storage.GetSubscribersFor(typeof(PingMessage));
            pingSubscribers.ShouldHaveTheSameElementsAs(sub1, sub3);

            var pongSubscribers = _storage.GetSubscribersFor(typeof(PongMessage));
            pongSubscribers.ShouldHaveTheSameElementsAs(sub2);
        }

        [Fact]
        public void active_subscriptions()
        {
            _storage.PersistSubscriptions(_subscriptions);

            _storage.ActiveSubscriptions.ShouldHaveCount(3);
            _storage.ActiveSubscriptions
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));
        }
    }
}
