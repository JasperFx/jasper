using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Consul.Internal;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Consul.Testing
{
    public class ConsulSubscriptionRepositoryTests : IDisposable
    {
        private readonly JasperRuntime _runtime;
        private ISubscriptionsRepository theRepository;

        public ConsulSubscriptionRepositoryTests()
        {
            using (var client = new ConsulClient())
            {
                client.KV.DeleteTree(ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX).Wait();
            }

            var registry = new JasperBusRegistry();
            registry.ServiceName = "ConsulSampleApp";

            registry.Services.For<ISubscriptionsRepository>()
                .Use<ConsulSubscriptionRepository>();

            _runtime = JasperRuntime.For(registry);

            theRepository = _runtime.Container.GetInstance<ISubscriptionsRepository>();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        [Fact]
        public async Task persist_and_load_subscriptions()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(BlueMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(RedMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(OrangeMessage)){Role = SubscriptionRole.Subscribes},
            };

            subscriptions.Each(x => x.NodeName = "ConsulSampleApp");

            await theRepository.PersistSubscriptions(subscriptions);

            var publishes = await theRepository.LoadSubscriptions(SubscriptionRole.Publishes);
            publishes.Count().ShouldBe(3);

            publishes.Any(x => x.MessageType == typeof(GreenMessage).ToTypeAlias()).ShouldBeTrue();
            publishes.Any(x => x.MessageType == typeof(RedMessage).ToTypeAlias()).ShouldBeTrue();
            publishes.Any(x => x.MessageType == typeof(BlueMessage).ToTypeAlias()).ShouldBeTrue();
        }

        [Fact]
        public async Task find_subscriptions_for_a_message_type()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(GreenMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(GreenMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(BlueMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(RedMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(OrangeMessage)){Role = SubscriptionRole.Subscribes},
            };

            subscriptions.Each(x => x.NodeName = "ConsulSampleApp");

            await theRepository.PersistSubscriptions(subscriptions);

            var greens = await theRepository.GetSubscribersFor(typeof(GreenMessage));
            greens.Length.ShouldBe(3);
        }

        [Fact]
        public async Task remove_subscriptions()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(BlueMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(RedMessage)){Role = SubscriptionRole.Publishes},
                new Subscription(typeof(OrangeMessage)){Role = SubscriptionRole.Subscribes},
            };

            subscriptions.Each(x => x.NodeName = "ConsulSampleApp");

            await theRepository.PersistSubscriptions(subscriptions);

            await theRepository.RemoveSubscriptions(new List<Subscription>{subscriptions[1], subscriptions[2]});


            (await theRepository.LoadSubscriptions(SubscriptionRole.Publishes)).Single()
                .MessageType.ShouldBe(typeof(GreenMessage).ToTypeAlias());

        }


    }

    public class GreenMessage
    {

    }

    public class BlueMessage
    {

    }

    public class RedMessage
    {

    }

    public class OrangeMessage{}
}
