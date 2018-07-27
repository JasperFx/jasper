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
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    [Collection("Consul")]
    public class ConsulNodeDiscoveryTests : IDisposable
    {
        private readonly JasperRuntime _runtime;
        private readonly INodeDiscovery theNodeDiscovery;

        public ConsulNodeDiscoveryTests()
        {
            using (var client = new ConsulClient())
            {
                client.KV.DeleteTree(ConsulNodeDiscovery.TRANSPORTNODE_PREFIX).Wait();
            }

            var registry = new JasperRegistry
            {
                ServiceName = "ConsulTestApp"
            };

            registry.Services.ForSingletonOf<INodeDiscovery>().Use<ConsulNodeDiscovery>();

            _runtime = JasperRuntime.For(registry);

            theNodeDiscovery = _runtime.Get<INodeDiscovery>().As<ConsulNodeDiscovery>();

        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        [Fact]
        public async Task successfully_registers_itself_as_an_active_node()
        {
            theNodeDiscovery.LocalNode.ShouldNotBeNull();

            var peers = await theNodeDiscovery.FindPeers();

            peers.Single().ServiceName.ShouldBe("ConsulTestApp");

            using (var settings = new ConsulSettings())
            {
                var nodes = await settings.Client.KV.List(ConsulNodeDiscovery.TRANSPORTNODE_PREFIX);
                nodes.Response.Length.ShouldBe(1);
            }
        }

        [Fact]
        public async Task successfully_unregister_on_shutdown()
        {
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
