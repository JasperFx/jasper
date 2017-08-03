using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using Jasper.Testing.Bus;
using JasperBus.Marten.Tests.Setup;
using Marten;
using Shouldly;
using Xunit;
using PingMessage = Jasper.Testing.Bus.Samples.PingMessage;

namespace JasperBus.Marten.Tests
{
    public class NodeDiscoveryTests : IntegrationContext
    {
        private readonly SubContext _serviceEndpoint1;
        private readonly SubContext _serviceEndpoint2;
        private readonly Uri _clientUri = "jasper://localhost:6000/client".ToUri();
        private readonly Uri _primaryServiceUri = "jasper://localhost:6001/service".ToUri();
        private readonly Uri _secondaryServiceUri = "jasper://localhost:6002/service".ToUri();

        public NodeDiscoveryTests()
        {
            with(_ =>
            {
                _.ServiceName = "Client";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.Channels.ListenForMessagesFrom(_clientUri);
            });

            _serviceEndpoint1 = new SubContext(_ =>
            {
                _.ServiceName = "Service";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.Channels.ListenForMessagesFrom(_primaryServiceUri);
            });

            _serviceEndpoint2 = new SubContext(_ =>
            {
                _.ServiceName = "Service";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<EnvironmentSettings>(x => x.MachineName = "localhost");
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.Channels.ListenForMessagesFrom(_secondaryServiceUri);
            });
        }

        [Fact]
        public async Task add_dynamic_subscription_updates_all_nodes()
        {
            var bus = Runtime.Container.GetInstance<IServiceBus>();

            await bus.Send(_primaryServiceUri, new SubscriptionRequested
            {
                Subscriptions = new[]
                {
                    new Subscription(typeof(PingMessage))
                    {
                        Id = Guid.NewGuid(),
                        Publisher = "Client",
                        Role = SubscriptionRole.Subscribes,
                        Receiver = _clientUri,
                        Source = "jasper://loadbalancer:5000/service".ToUri()
                    }
                }
            });

            var history = _serviceEndpoint2.Runtime.Container.GetInstance<MessageHistory>();
            await history.WaitFor<SubscriptionsChanged>();

            var se1Subscriptions = _serviceEndpoint1.Runtime.Container.GetInstance<ISubscriptionsRepository>();
            (await se1Subscriptions.GetActiveSubscriptions()).Count().ShouldBe(1, "Primary endpoint missing subscription");

            var se2Subscriptions = _serviceEndpoint2.Runtime.Container.GetInstance<ISubscriptionsRepository>();
            (await se2Subscriptions.GetActiveSubscriptions()).Count().ShouldBe(1, "Secondary endpoint missing subscription");
        }

        public override void Dispose()
        {
            var docStore = Runtime.Container.GetInstance<IDocumentStore>();
            docStore.Advanced.Clean.CompletelyRemoveAll();

            base.Dispose();
            _serviceEndpoint1.Dispose();
            _serviceEndpoint2.Dispose();
        }
    }
}
