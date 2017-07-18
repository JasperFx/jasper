using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionsStorageTester
    {
        private readonly ISubscriptionsStorage _storage;
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
            _repository = new InMemorySubscriptionsRepository();
            _storage = new SubscriptionsStorage(_repository);
        }

        [Fact]
        public async Task persists_subscriptions()
        {
            await _storage.PersistSubscriptions(_subscriptions);

            (await _repository.LoadSubscriptions(SubscriptionRole.Publishes))
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));

            (await _repository.LoadSubscriptions(SubscriptionRole.Subscribes))
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Subscribes));

        }

        [Fact]
        public async Task removes_subscriptions()
        {
            await _storage.PersistSubscriptions(_subscriptions);

            await _storage.RemoveSubscriptions(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes).ToArray());

            (await _repository.LoadSubscriptions(SubscriptionRole.Publishes))
                .ShouldHaveCount(0);

        }

        [Fact]
        public async Task adds_missing_subscriptions_to_cache()
        {
            await _storage.PersistSubscriptions(_subscriptions);

            await _repository.PersistSubscriptions(new []
            {
                Subs.NewSubscription(role:SubscriptionRole.Publishes)
            });

            var expectedPublish = _subscriptions.Count(x => x.Role == SubscriptionRole.Publishes) + 1;

            (await _storage.LoadSubscriptions(SubscriptionRole.Publishes))
                .ShouldHaveCount(expectedPublish);

        }

        [Fact]
        public async Task subscribers_for_message()
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

            await _storage.PersistSubscriptions(new[] {sub1, sub2, sub3});

            var pingSubscribers = await _storage.GetSubscribersFor(typeof(PingMessage));
            pingSubscribers.ShouldHaveTheSameElementsAs(sub1, sub3);

            var pongSubscribers = await _storage.GetSubscribersFor(typeof(PongMessage));
            pongSubscribers.ShouldHaveTheSameElementsAs(sub2);
        }

        [Fact]
        public async Task active_subscriptions()
        {
            await _storage.PersistSubscriptions(_subscriptions);

            (await _storage.GetActiveSubscriptions()).ShouldHaveCount(3);
            (await _storage.GetActiveSubscriptions())
                .ShouldHaveTheSameElementsAs(_subscriptions.Where(x => x.Role == SubscriptionRole.Publishes));
        }
    }
}
