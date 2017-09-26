using BlueMilk;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Durable;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Bus.Transports.Loopback;
using Jasper.Configuration;
using Jasper.Conneg;

namespace Jasper.Bus
{
    internal class ServiceBusRegistry : ServiceRegistry
    {
        internal ServiceBusRegistry()
        {
            ForSingletonOf<ITransport>()
                .Use<LoopbackTransport>()
                .Use<LightweightTransport>()
                .Use<DurableTransport>();



            For<IEnvelopeSender>().Use<EnvelopeSender>();
            For<IServiceBus>().Use<ServiceBus>();
            For<IHandlerPipeline>().Use<HandlerPipeline>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();

            For<ISerializerFactory>().Use<NewtonsoftSerializerFactory>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();

            For<IUriLookup>().Use<ConfigUriLookup>();

            ForSingletonOf<SerializationGraph>().Use<SerializationGraph>();

            ForSingletonOf<IMessageRouter>().Use<MessageRouter>();

            ForSingletonOf<UriAliasLookup>().Use<UriAliasLookup>();

            For<IPersistence>().Use<NulloPersistence>();


        }
    }
}
