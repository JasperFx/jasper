using Jasper.Configuration;
using Jasper.Consul.Internal;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Jasper.Consul
{
    /// <summary>
    ///     Explicitly applied extension to register Consul-based subscriptions
    ///     and node discovery
    /// </summary>
    public class ConsulBackedSubscriptions : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.ForSingletonOf<ISubscriptionsRepository>().Use<ConsulSubscriptionRepository>();
            registry.Services.ForSingletonOf<INodeDiscovery>().Use<ConsulNodeDiscovery>();
        }
    }
}
