using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Runtime.Serializers;
using JasperBus.Transports.InMemory;
using JasperBus.Runtime.Subscriptions;
using JasperBus.Transports.LightningQueues;
using StructureMap;

namespace JasperBus
{
    internal class ServiceBusRegistry : Registry
    {
        internal ServiceBusRegistry()
        {
            For<ITransport>().Singleton().Add<LightningQueuesTransport>();
            For<ITransport>().Singleton().Add<InMemoryTransport>();
            For<IEnvelopeSender>().Use<EnvelopeSender>();
            For<IServiceBus>().Use<ServiceBus>();
            For<IHandlerPipeline>().Use<HandlerPipeline>();
            For<ISubscriptionActivator>().Use<SubscriptionActivator>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsCache>().UseIfNone<SubscriptionsCache>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();
            ForSingletonOf<ISubscriptionsStorage>().UseIfNone<SubscriptionsStorage>();

            ForSingletonOf<IEnvelopeSerializer>().Use<EnvelopeSerializer>();
            For<IMessageSerializer>().Add<JsonMessageSerializer>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();
        }
    }
}
