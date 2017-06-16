using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.InMemory;
using Jasper.Bus.Transports.LightningQueues;
using Jasper.Configuration;
using StructureMap;

namespace Jasper.Bus
{
    internal class ServiceBusRegistry : ServiceRegistry
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
