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
    public class ConsulBackedRegistrationsTests : ConsulContext
    {
        [Fact]
        public void use_the_extension()
        {
            var registry = new JasperRegistry();
            registry.Include<ConsulBackedSubscriptions>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<INodeDiscovery>().ShouldBeOfType<ConsulNodeDiscovery>();
                runtime.Get<ISubscriptionsRepository>()
                    .ShouldBeOfType<ConsulSubscriptionRepository>();
            }
        }

        public ConsulBackedRegistrationsTests(DockerFixture<ConsulContainer> fixture) : base(fixture)
        {
        }
    }

    // SAMPLE: AppUsingConsulBackedSubscriptions
    public class AppUsingConsulBackedSubscriptions : JasperRegistry
    {
        public AppUsingConsulBackedSubscriptions()
        {
            Include<ConsulBackedSubscriptions>();
        }
    }
    // ENDSAMPLE
}
