using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Consul.Internal;
using Shouldly;
using Xunit;

namespace Jasper.Consul.Testing
{
    public class ConsulBackedRegistrationsTests
    {
        [Fact]
        public void use_the_extension()
        {
            var registry = new JasperRegistry();
            registry.Include<ConsulBackedSubscriptions>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Container.GetInstance<INodeDiscovery>().ShouldBeOfType<ConsulNodeDiscovery>();
                runtime.Container.GetInstance<ISubscriptionsRepository>()
                    .ShouldBeOfType<ConsulSubscriptionRepository>();
            }
        }
    }
}
