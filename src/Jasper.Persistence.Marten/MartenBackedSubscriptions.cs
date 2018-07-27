using Jasper.Configuration;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Persistence.Marten.Subscriptions;

namespace Jasper.Persistence.Marten
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
