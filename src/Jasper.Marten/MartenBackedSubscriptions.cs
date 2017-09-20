using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Configuration;
using Jasper.Marten.Subscriptions;

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
