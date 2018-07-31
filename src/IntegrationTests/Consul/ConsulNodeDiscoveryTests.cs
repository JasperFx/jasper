using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Baseline;
using Consul;
using Jasper;
using Jasper.Consul;
using Jasper.Consul.Internal;
using Jasper.Messaging.Runtime.Subscriptions;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    public class ConsulNodeDiscoveryTests : ConsulContext, IDisposable
    {
        private JasperRuntime _runtime;
        private INodeDiscovery theNodeDiscovery;

        public ConsulNodeDiscoveryTests(DockerFixture<ConsulContainer> container) : base(container)
        {


        }

        private async Task withApp()
        {
            using (var client = new ConsulClient())
            {
                await client.KV.DeleteTree(ConsulNodeDiscovery.TRANSPORTNODE_PREFIX);
            }

            var registry = new JasperRegistry
            {
                ServiceName = "ConsulTestApp"
            };

            registry.Services.ForSingletonOf<INodeDiscovery>().Use<ConsulNodeDiscovery>();

            _runtime = await JasperRuntime.ForAsync(registry);

            theNodeDiscovery = _runtime.Get<INodeDiscovery>().As<ConsulNodeDiscovery>();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        [Fact]
        public async Task successfully_registers_itself_as_an_active_node()
        {
            await withApp();

            theNodeDiscovery.LocalNode.ShouldNotBeNull();

            var peers = await theNodeDiscovery.FindPeers();

            peers.Single().ServiceName.ShouldBe("ConsulTestApp");

            using (var settings = new ConsulSettings())
            {
                var nodes = await settings.Client.KV.List(ConsulNodeDiscovery.TRANSPORTNODE_PREFIX);
                nodes.Response.Length.ShouldBeGreaterThanOrEqualTo(1);
            }
        }

        [Fact]
        public async Task successfully_unregister_on_shutdown()
        {
            await withApp();

            _runtime.Dispose();

            using (var settings = new ConsulSettings())
            {
                var nodes = await settings.Client.KV.List(ConsulNodeDiscovery.TRANSPORTNODE_PREFIX);
                nodes.StatusCode.ShouldBe(HttpStatusCode.NotFound);
                nodes.Response.ShouldBeNull();
            }
        }
    }
}
