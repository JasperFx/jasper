using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Subscriptions;
using Jasper.Util;
using Marten;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public class node_discovery_registration_and_unregistration : MartenContext, IDisposable
    {
        public node_discovery_registration_and_unregistration()
        {

            using (var store = DocumentStore.For(Servers.PostgresConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Settings.Alter<MartenSubscriptionSettings>(x =>
                    x.StoreOptions.Connection(Servers.PostgresConnectionString));

                _.Include<MartenBackedSubscriptions>();

                _.ServiceName = "MartenSampleApp";

                _.Settings.Alter<MessagingSettings>(x => { x.MachineName = "MyBox"; });

                _.Settings.Alter<StoreOptions>(x => { x.Connection(Servers.PostgresConnectionString); });

                _.Transports.LightweightListenerAt(2345);
            });


            theRepository = _runtime.Get<ISubscriptionsRepository>();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private JasperRuntime _runtime;
        private ISubscriptionsRepository theRepository;

        [Fact]
        public async Task find_all_nodes()
        {
            var node1 = new ServiceNode
            {
                Id = "a1",
                ServiceName = "a"
            };

            var node2 = new ServiceNode
            {
                Id = "a2",
                ServiceName = "a"
            };

            var node3 = new ServiceNode
            {
                Id = "b1",
                ServiceName = "b"
            };

            var node4 = new ServiceNode
            {
                Id = "c1",
                ServiceName = "c"
            };

            using (var session = _runtime.Get<IDocumentStore>().LightweightSession())
            {
                session.Store(node1, node2, node3, node4);
                await session.SaveChangesAsync();
            }

            var nodeDiscovery = _runtime.Get<INodeDiscovery>();
            var all = await nodeDiscovery.FindAllKnown();

            // 4 + the node for the currently running app
            all.Length.ShouldBe(5);
        }

        [Fact]
        public void should_store_app_information()
        {
            using (var session = _runtime.Get<IDocumentStore>().OpenSession())
            {
                var uri = "tcp://localhost:2345".ToUri().ToMachineUri();

                session.Load<ServiceNode>("MartenSampleApp@MyBox")
                    .TcpEndpoints.ShouldContain(uri);
            }
        }

        [Fact]
        public void should_unregister_on_shutdown()
        {
            _runtime.Dispose();
            _runtime = null;

            using (var store = DocumentStore.For(Servers.PostgresConnectionString))
            {
                using (var session = store.OpenSession())
                {
                    session.Query<ServiceNode>().Count()
                        .ShouldBe(0);
                }
            }
        }
    }
}
