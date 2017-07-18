using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Configuration;
using Jasper.Consul.Internal;

namespace Jasper.Consul
{
    public class ConsulBackedSubscriptions : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.ForSingletonOf<ISubscriptionsRepository>().Use<ConsulSubscriptionRepository>();
            registry.Services.ForSingletonOf<INodeDiscovery>().Use<ConsulNodeDiscovery>();
        }
    }
}
