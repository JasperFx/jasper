using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Consul;
using Shouldly;
using Xunit;

namespace IntegrationTests.Consul
{
    public class using_consul_based_lookup : IDisposable
    {

        private JasperRuntime theRuntime;

        public using_consul_based_lookup()
        {

            var registry = new ConsulUsingApp();



            seedData(registry).Wait();

            theRuntime = JasperRuntime.For(registry);
        }

        private async Task seedData(ConsulUsingApp registry)
        {
            var gateway = new ConsulGateway(new HttpClient(), new ConsulSettings());
            await gateway.SetProperty(registry.prop1, "jasper://localhost:2345/queue1");
            await gateway.SetProperty(registry.prop2, "jasper://localhost:2345/queue2");
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }

        [Fact]
        public void should_look_up_actual_uri_from_consul()
        {
            var channels = theRuntime.Container.GetInstance<ChannelGraph>();

            channels.HasChannel("jasper://localhost:2345/queue1".ToUri()).ShouldBeTrue();
            channels.HasChannel("jasper://localhost:2345/queue2".ToUri()).ShouldBeTrue();
        }
    }

    public class ConsulUsingApp : JasperBusRegistry
    {
        public string prop1 = Guid.NewGuid().ToString();
        public string prop2 = Guid.NewGuid().ToString();

        public ConsulUsingApp()
        {
            ListenForMessagesFrom("consul://" + prop1);
            ListenForMessagesFrom("consul://" + prop2);
        }
    }
}
