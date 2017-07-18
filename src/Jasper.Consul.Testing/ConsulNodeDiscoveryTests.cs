using System;
using System.Linq;
using Baseline;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Consul.Internal;
using Shouldly;
using Xunit;

namespace Jasper.Consul.Testing
{
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

            var registry = new JasperBusRegistry
            {
                ServiceName = "ConsulTestApp"
            };

            registry.Services.ForSingletonOf<INodeDiscovery>().Use<ConsulNodeDiscovery>();

            _runtime = JasperRuntime.For(registry);

            theNodeDiscovery = _runtime.Container.GetInstance<INodeDiscovery>().As<ConsulNodeDiscovery>();

        }

        public void Dispose()
        {
            _runtime.Dispose();
        }

        [Fact]
        public void successfully_registers_itself_as_an_active_node()
        {
            theNodeDiscovery.LocalNode.ShouldNotBeNull();

            var peers = theNodeDiscovery.FindPeers();

            peers.Single().NodeName.ShouldBe("ConsulTestApp");
        }
    }
}
