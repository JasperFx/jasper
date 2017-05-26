﻿using System;
using System.Linq;
using System.Threading.Tasks;
using JasperBus.Marten.Tests.Setup;
using JasperBus.Runtime;
using JasperBus.Runtime.Subscriptions;
using JasperBus.Tests;
using JasperBus.Tests.Samples;
using JasperBus.Tracking;
using JasperBus.Transports.LightningQueues;
using Marten;
using Shouldly;
using Xunit;

namespace JasperBus.Marten.Tests
{
    public class NodeDiscoveryTests : IntegrationContext
    {
        private readonly SubContext _serviceEndpoint1;
        private readonly SubContext _serviceEndpoint2;
        private readonly Uri _clientUri = "lq.tcp://localhost:6000/client".ToUri();
        private readonly Uri _primaryServiceUri = "lq.tcp://localhost:6001/service".ToUri();
        private readonly Uri _secondaryServiceUri = "lq.tcp://localhost:6002/service".ToUri();

        public NodeDiscoveryTests()
        {
            with(_ =>
            {
                _.NodeName = "Client";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.ListenForMessagesFrom(_clientUri);
            });

            _serviceEndpoint1 = new SubContext(_ =>
            {
                _.NodeName = "Service";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.ListenForMessagesFrom(_primaryServiceUri);
            });

            _serviceEndpoint2 = new SubContext(_ =>
            {
                _.NodeName = "Service";
                _.Services.IncludeRegistry<MartenSubscriptionRegistry>();
                _.Services.ForSingletonOf<MessageHistory>().Use(new MessageHistory());
                _.Services.AddService<IBusLogger, MessageTrackingLogger>();
                _.Settings.Alter<EnvironmentSettings>(x => x.MachineName = "localhost");
                _.Settings.Alter<MartenSubscriptionSettings>(x => x.ConnectionString = ConnectionSource.ConnectionString);
                _.ListenForMessagesFrom(_secondaryServiceUri);
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
                        NodeName = "Client",
                        Role = SubscriptionRole.Subscribes,
                        Receiver = _clientUri,
                        Source = "lq.tcp://loadbalancer:5000/service".ToUri()
                    }
                }
            });

            var history = _serviceEndpoint2.Runtime.Container.GetInstance<MessageHistory>();
            await history.WaitFor<SubscriptionsChanged>();

            var se1Subscriptions = _serviceEndpoint1.Runtime.Container.GetInstance<ISubscriptionsStorage>();
            se1Subscriptions.ActiveSubscriptions.Count().ShouldBe(1, "Primary endpoint missing subscription");

            var se2Subscriptions = _serviceEndpoint2.Runtime.Container.GetInstance<ISubscriptionsStorage>();
            se2Subscriptions.ActiveSubscriptions.Count().ShouldBe(1, "Secondary endpoint missing subscription");
        }

        public override void Dispose()
        {
            var docStore = Runtime.Container.GetInstance<IDocumentStore>();
            docStore.Advanced.Clean.CompletelyRemoveAll();

            base.Dispose();
            _serviceEndpoint1.Dispose();
            _serviceEndpoint2.Dispose();
            LightningQueuesTransport.DeleteAllStorage();
        }
    }
}
