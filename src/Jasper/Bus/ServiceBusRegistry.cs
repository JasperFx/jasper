using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.InMemory;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Configuration;
using Jasper.Conneg;

namespace Jasper.Bus
{
    internal class ServiceBusRegistry : ServiceRegistry
    {
        internal ServiceBusRegistry()
        {
            ForSingletonOf<IInMemoryQueue>().Use<InMemoryQueue>();

            For<ITransport>().Singleton().AddInstances(_ =>
            {
                _.Type<InMemoryTransport>();
                _.Type<LightweightTransport>();
            });


            For<IBusLogger>().Use<NulloBusLogger>();

            For<IEnvelopeSender>().Use<EnvelopeSender>();
            For<IServiceBus>().Use<ServiceBus>();
            For<IHandlerPipeline>().Use<HandlerPipeline>();
            For<ISubscriptionActivator>().Use<SubscriptionActivator>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();

            For<ISerializer>().Add<NewtonsoftSerializer>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();

            For<IUriLookup>().Add<ConfigUriLookup>();

            ForSingletonOf<SerializationGraph>().Use<SerializationGraph>();

            ForSingletonOf<IMessageRouter>().Use<MessageRouter>();

        }
    }
}
