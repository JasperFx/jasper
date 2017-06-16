using Jasper.Bus.Runtime.Subscriptions;
using Marten;
using StructureMap;

namespace JasperBus.Marten
{
    public class MartenSubscriptionRegistry : Registry
    {
        public MartenSubscriptionRegistry()
        {
            const string subscriptionsDocumentStoreName = "martenSubscriptionsDocumentStore";
            For<IDocumentStore>()
                .Use(context => DocumentStore
                    .For(context.GetInstance<MartenSubscriptionSettings>().ConnectionString))
                .Named(subscriptionsDocumentStoreName);

            ForSingletonOf<ISubscriptionsRepository>().Use<MartenSubscriptionRepository>()
                .Ctor<IDocumentStore>().Is(content => content.GetInstance<IDocumentStore>(subscriptionsDocumentStoreName));

            ForSingletonOf<INodeDiscovery>().Use<MartenNodeDiscovery>()
                .Ctor<IDocumentStore>().Is(context => context.GetInstance<IDocumentStore>(subscriptionsDocumentStoreName));

        }
    }
}
