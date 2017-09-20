using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Marten.Subscriptions;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Bus;
using Jasper.Util;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    // SAMPLE: AppWithMartenBackedSubscriptions
    public class AppWithMartenBackedSubscriptions : JasperRegistry
    {
        public AppWithMartenBackedSubscriptions()
        {
            // Use the Include() method so that Jasper can
            // get the order of precedence right between
            // an extension and the application settings
            Include<MartenBackedSubscriptions>();
        }
    }
    // ENDSAMPLE

    public class subscription_repository_functionality : IDisposable
    {
        private JasperRuntime _runtime;
        private ISubscriptionsRepository theRepository;
        private Uri theDestination = "something://localhost:3333/here".ToUri();

        public subscription_repository_functionality()
        {

            using (var store = DocumentStore.For(ConnectionSource.ConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            _runtime = JasperRuntime.For(_ =>
            {
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.StoreOptions.Connection(ConnectionSource.ConnectionString));

                _.Include<MartenBackedSubscriptions>();

                _.ServiceName = "MartenSampleApp";
            });



            theRepository = _runtime.Container.GetInstance<ISubscriptionsRepository>();
        }

        [Fact]
        public async Task persist_and_load_subscriptions()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage), theDestination),
                new Subscription(typeof(BlueMessage), theDestination),
                new Subscription(typeof(RedMessage), theDestination),
                new Subscription(typeof(OrangeMessage), theDestination),
            };

            subscriptions.Each(x => x.ServiceName = "MartenSampleApp");

            await theRepository.PersistSubscriptions(subscriptions);

            var publishes = await theRepository.GetSubscribersFor(typeof(GreenMessage));

            publishes.Count().ShouldBe(1);

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

            subscriptions.Each(x => x.ServiceName = "MartenSampleApp");

            await theRepository.PersistSubscriptions(subscriptions);

            var greens = await theRepository.GetSubscribersFor(typeof(GreenMessage));
            greens.Length.ShouldBe(3);
        }


        [Fact]
        public async Task replace_subscriptions_for_a_service()
        {
            var subscriptions = new Subscription[]
            {
                new Subscription(typeof(GreenMessage), "something://localhost:3333/here".ToUri()){ServiceName = "One"},
                new Subscription(typeof(GreenMessage), "something://localhost:4444/here".ToUri()){ServiceName = "Two"},
                new Subscription(typeof(GreenMessage), "something://localhost:5555/here".ToUri()){ServiceName = "Two"},
                new Subscription(typeof(BlueMessage), theDestination){ServiceName = "One"},
                new Subscription(typeof(RedMessage), theDestination){ServiceName = "One"},
                new Subscription(typeof(OrangeMessage), theDestination){ServiceName = "One"},
            };

            await theRepository.PersistSubscriptions(subscriptions);

            var replacements = new Subscription[]
            {
                new Subscription(typeof(GreenMessage), "something://localhost:3335/here".ToUri()){ServiceName = "One"},
                new Subscription(typeof(BlueMessage), theDestination){ServiceName = "One"},
            };

            await theRepository.ReplaceSubscriptions("One", replacements);

            var known = await theRepository.As<MartenSubscriptionRepository>().AllSubscriptions();

            var ones = known.Where(x => x.ServiceName == "One").ToArray();

            ones.Length.ShouldBe(2);
            ones.Select(x => x.MessageType).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(typeof(BlueMessage).ToMessageAlias(), typeof(GreenMessage).ToMessageAlias());
        }

        public void Dispose()
        {
            _runtime.Dispose();
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
