using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Consul.Internal;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Consul.Testing
{
    [Collection("Consul")]
    public class ConsulSubscriptionRepositoryTests : IDisposable
    {
        private Uri theDestination = "something://localhost:3333/here".ToUri();

        private readonly JasperRuntime _runtime;
        private ISubscriptionsRepository theRepository;

        public ConsulSubscriptionRepositoryTests()
        {
            using (var client = new ConsulClient())
            {
                client.KV.DeleteTree(ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX).Wait();
            }

            var registry = new JasperRegistry();
            registry.ServiceName = "ConsulSampleApp";

            registry.Services.For<ISubscriptionsRepository>()
                .Use<ConsulSubscriptionRepository>();

            _runtime = JasperRuntime.For(registry);

            theRepository = _runtime.Get<ISubscriptionsRepository>();
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
                new Subscription(typeof(GreenMessage), theDestination)
                {
                    Accept = new string[]{"application/json"},

                },
                new Subscription(typeof(BlueMessage), theDestination),
                new Subscription(typeof(RedMessage), theDestination),
                new Subscription(typeof(OrangeMessage), theDestination),
            };

            var capabilities = new ServiceCapabilities
            {
                Subscriptions = subscriptions,
                Published = new PublishedMessage[0]
            };

            subscriptions.Each(x => x.ServiceName = "ConsulSampleApp");

            await theRepository.PersistCapabilities(capabilities);

            var publishes = await theRepository.GetSubscribersFor(typeof(GreenMessage));

            publishes.Count().ShouldBe(1);

            publishes.Single().Accept.ShouldHaveTheSameElementsAs("application/json");

            publishes.Any(x => x.MessageType == typeof(GreenMessage).ToMessageAlias()).ShouldBeTrue();
        }

        [Fact]
        public async Task find_subscriptions_for_a_message_type()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage), "something://localhost:3333/here".ToUri()){},
                new Subscription(typeof(GreenMessage), "something://localhost:4444/here".ToUri()){},
                new Subscription(typeof(GreenMessage), "something://localhost:5555/here".ToUri()){},
                new Subscription(typeof(BlueMessage), theDestination){},
                new Subscription(typeof(RedMessage), theDestination){},
                new Subscription(typeof(OrangeMessage), theDestination){},
            };

            subscriptions.Each(x => x.ServiceName = "ConsulSampleApp");

            var capabilities = new ServiceCapabilities
            {
                Subscriptions = subscriptions,
                Published = new PublishedMessage[0]
            };

            await theRepository.PersistCapabilities(capabilities);

            var greens = await theRepository.GetSubscribersFor(typeof(GreenMessage));
            greens.Length.ShouldBe(3);
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
