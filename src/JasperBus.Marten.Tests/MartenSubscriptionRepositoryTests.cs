using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using Jasper.Testing.Bus;
using Jasper.Util;
using JasperBus.Marten.Tests.Setup;
using Marten;
using Xunit;
using Shouldly;
using TestMessages;

namespace JasperBus.Marten.Tests
{

    public class stores_subscriptions_in_marten : IntegrationContext
    {
        private readonly SubContext _subContext;
        private readonly Uri _primaryQueueUri = "jasper://localhost:7001/primary".ToUri();
        private readonly Uri _secondaryQueueUri = "jasper://localhost:7002/secondary".ToUri();
        private readonly MessageHistory _secondaryHistory;
        private readonly MessageHistory _primaryHistory;
        private readonly Task _subscriptionsSetupTask;

        public stores_subscriptions_in_marten()
        {
            _primaryHistory = new MessageHistory();
            _secondaryHistory = new MessageHistory();
            _subscriptionsSetupTask = waitForSubscriptionMessages();
            with(_ =>
            {
                _.ServiceName = "Primary";
                _.Services.For<IBusLogger>().Add(new MessageTrackingLogger(_primaryHistory));
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();

                _.Settings.Alter<MartenSubscriptionSettings>(x =>
                    x.ConnectionString = ConnectionSource.ConnectionString);

                _.Channels.ListenForMessagesFrom(_primaryQueueUri);
                _.SubscribeLocally()
                    .ToSource(_secondaryQueueUri)
                    .ToMessage<PongMessage>();
            });

            _subContext = new SubContext(_ =>
            {
                _.ServiceName = "Secondary";
                _.Services.For<IBusLogger>().Add(new MessageTrackingLogger(_secondaryHistory));
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();

                _.Settings.Alter<MartenSubscriptionSettings>(x =>
                    x.ConnectionString = ConnectionSource.ConnectionString);

                _.Channels.ListenForMessagesFrom(_secondaryQueueUri);
                _.SubscribeLocally()
                    .ToSource(_primaryQueueUri)
                    .ToMessage<PingMessage>();
            });
        }

        [Fact]
        public async Task published_subscriptions()
        {
            await _subscriptionsSetupTask;

            var subscriptions = await this.GetPublishedSubscriptions();
            subscriptions.ShouldHaveCount(1);
            (await this.GetActiveSubscriptions()).ShouldHaveTheSameElementsAs(subscriptions);
            var sub1 = subscriptions.First();
            sub1.NodeName.ShouldBe("Primary");
            sub1.VerifySubscription<PingMessage>(source: _primaryQueueUri, destination: _secondaryQueueUri);

            var subsSubscriptions = await _subContext.GetPublishedSubscriptions();
            subsSubscriptions.ShouldHaveCount(1);
            (await _subContext.GetActiveSubscriptions()).ShouldHaveTheSameElementsAs(subsSubscriptions);
            var sub2 = subsSubscriptions.First();
            sub2.NodeName.ShouldBe("Secondary");
            sub2.VerifySubscription<PongMessage>(source: _secondaryQueueUri, destination: _primaryQueueUri);
        }

        [Fact]
        public async Task subscribed_subscriptions()
        {
            await _subscriptionsSetupTask;

            var subscriptions = await this.GetSubscribedSubscriptions();
            subscriptions.ShouldHaveCount(1);
            var sub1 = subscriptions.First();
            sub1.NodeName.ShouldBe("Primary");
            sub1.VerifySubscription<PongMessage>(source: _secondaryQueueUri, destination: _primaryQueueUri,
                role: SubscriptionRole.Subscribes);

            var subsSubscriptions = await _subContext.GetSubscribedSubscriptions();
            subsSubscriptions.ShouldHaveCount(1);
            var sub2 = subsSubscriptions.First();
            sub2.NodeName.ShouldBe("Secondary");
            sub2.VerifySubscription<PingMessage>(source: _primaryQueueUri, destination: _secondaryQueueUri,
                role: SubscriptionRole.Subscribes);
        }

        private async Task waitForSubscriptionMessages()
        {
            var primaryTask = _primaryHistory.WaitFor<SubscriptionRequested>();
            var secondaryTask = _secondaryHistory.WaitFor<SubscriptionRequested>();
            await Task.WhenAll(primaryTask, secondaryTask);
        }

        public override void Dispose()
        {
            var docStore = Runtime.Container.GetInstance<IDocumentStore>();
            docStore.Advanced.Clean.CompletelyRemoveAll();

            base.Dispose();
            _subContext.Dispose();
        }
    }

    public class SubContext : IntegrationContext
    {
        public SubContext(Action<JasperBusRegistry> setup)
        {
            with(setup);
        }
    }

    public static class Extensions
    {
        public static Task<Subscription[]> GetActiveSubscriptions(this IntegrationContext ctx)
        {
            return ctx.Runtime.Container.GetInstance<ISubscriptionsRepository>().GetActiveSubscriptions();
        }

        public static Task<Subscription[]> GetPublishedSubscriptions(this IntegrationContext ctx)
        {
            return
                ctx.Runtime.Container.GetInstance<ISubscriptionsRepository>()
                    .LoadSubscriptions(SubscriptionRole.Publishes);
        }

        public static Task<Subscription[]> GetSubscribedSubscriptions(this IntegrationContext ctx)
        {
            return
                ctx.Runtime.Container.GetInstance<ISubscriptionsRepository>()
                    .LoadSubscriptions(SubscriptionRole.Subscribes);
        }

        public static void VerifySubscription<TMessageType>(this Subscription subscription,
            Uri source,
            Uri destination,
            SubscriptionRole role = SubscriptionRole.Publishes)
        {
            subscription.Role.ShouldBe(role);
            subscription.MessageType.ShouldBe(typeof(TMessageType).FullName);
            subscription.Source.ShouldBe(role == SubscriptionRole.Publishes ? source.ToMachineUri() : source);
            subscription.Receiver.ShouldBe(destination.ToMachineUri());
        }
    }
}
