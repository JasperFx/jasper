using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper;
using Jasper.Consul;
using Jasper.Consul.Internal;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Util;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    public class using_consul_based_lookup : ConsulContext, IDisposable
    {

        private JasperRuntime theRuntime;

        public using_consul_based_lookup(DockerFixture<ConsulContainer> container) : base(container)
        {
            var registry = new ConsulUsingApp();



            seedData(registry).Wait();

            theRuntime = JasperRuntime.For(registry);
        }

        private async Task seedData(ConsulUsingApp registry)
        {
            var gateway = new ConsulGateway(new ConsulSettings());
            await gateway.SetProperty("one", "tcp://localhost:2345/queue1");
            await gateway.SetProperty("two", "tcp://localhost:2345/queue2");
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public void should_look_up_actual_uri_from_consul()
        {
            var channels = theRuntime.Get<IChannelGraph>();


            var lookups = theRuntime.Get<UriAliasLookup>();
            lookups.Resolve("consul://one".ToUri()).ShouldBe("tcp://localhost:2345/queue1".ToUri());
            lookups.Resolve("consul://two".ToUri()).ShouldBe("tcp://localhost:2345/queue2".ToUri());
        }
    }

    // SAMPLE: Using-Consul-Uri-Lookup
    public class ConsulUsingApp : JasperRegistry
    {
        public ConsulUsingApp()
        {
            // These calls are looking up values from the Key/Value
            // store in Consul
            Transports.ListenForMessagesFrom("consul://one");
            Transports.ListenForMessagesFrom("consul://two");
        }
    }
    // ENDSAMPLE
}
