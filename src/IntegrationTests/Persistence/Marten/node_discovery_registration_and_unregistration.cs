using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Subscriptions;
using Jasper.Util;
using Marten;
using Servers;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class node_discovery_registration_and_unregistration : MartenContext, IDisposable
    {
        public node_discovery_registration_and_unregistration(DockerFixture<MartenContainer> fixture) : base(fixture)
        {

            using (var store = DocumentStore.For(MartenContainer.ConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Settings.Alter<MartenSubscriptionSettings>(x =>
                    x.StoreOptions.Connection(MartenContainer.ConnectionString));

                _.Include<MartenBackedSubscriptions>();

                _.ServiceName = "MartenSampleApp";

                _.Settings.Alter<MessagingSettings>(x => { x.MachineName = "MyBox"; });

                _.Settings.Alter<StoreOptions>(x => { x.Connection(MartenContainer.ConnectionString); });

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
                session.Load<ServiceNode>("MartenSampleApp@MyBox")
                    .TcpEndpoints.ShouldContain($"tcp://{Environment.MachineName}:2345".ToUri());
            }
        }

        [Fact]
        public void should_unregister_on_shutdown()
        {
            _runtime.Dispose();
            _runtime = null;

            using (var store = DocumentStore.For(MartenContainer.ConnectionString))
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
