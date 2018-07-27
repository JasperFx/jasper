using Jasper.Configuration;
using Jasper.Marten.Subscriptions;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Jasper.Marten
{
    public class MartenBackedSubscriptions : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.ForSingletonOf<ISubscriptionsRepository>().Use<MartenSubscriptionRepository>();
            registry.Services.ForSingletonOf<INodeDiscovery>().Use<MartenNodeDiscovery>();
        }
    }
}
