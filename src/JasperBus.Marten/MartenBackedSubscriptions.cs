using Jasper;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Configuration;

namespace JasperBus.Marten
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
