using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Marten.Subscriptions;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing;
using Jasper.Util;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
{
    public class node_discovery_registration_and_unregistration : IDisposable
    {
        private JasperRuntime _runtime;
        private ISubscriptionsRepository theRepository;

        public node_discovery_registration_and_unregistration()
        {
            using (var store = DocumentStore.For(ConnectionSource.ConnectionString))
            {
                store.Advanced.Clean.CompletelyRemoveAll();
            }

            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();

                _.Settings.Alter<MartenSubscriptionSettings>(x => x.StoreOptions.Connection(ConnectionSource.ConnectionString));

                _.Include<MartenBackedSubscriptions>();

                _.ServiceName = "MartenSampleApp";

                _.Settings.Alter<BusSettings>(x =>
                {
                    x.MachineName = "MyBox";
                });

                _.Settings.Alter<StoreOptions>(x =>
                {
                    x.Connection(ConnectionSource.ConnectionString);
                });

                _.Transports.LightweightListenerAt(2345);
            });



            theRepository = _runtime.Get<ISubscriptionsRepository>();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

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

            using (var store = DocumentStore.For(ConnectionSource.ConnectionString))
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
