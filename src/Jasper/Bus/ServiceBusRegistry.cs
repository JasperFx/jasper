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
using Jasper.Conneg.Json;
using Jasper.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Bus
{
    internal class ServiceBusRegistry : ServiceRegistry
    {
        internal ServiceBusRegistry()
        {

            ForSingletonOf<ITransport>()
                .Use<LoopbackTransport>();

            ForSingletonOf<ITransport>()
                .Use<LightweightTransport>();

            ForSingletonOf<ITransport>()
                .Use<DurableTransport>();

            ForSingletonOf<ObjectPoolProvider>().Use<DefaultObjectPoolProvider>();


            For<IEnvelopeSender>().Use<EnvelopeSender>();
            For<IServiceBus>().Use<ServiceBus>();
            For<IHandlerPipeline>().Use<HandlerPipeline>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();

            For<IUriLookup>().Use<ConfigUriLookup>();

            ForSingletonOf<BusMessageSerializationGraph>().Use<BusMessageSerializationGraph>();

            ForSingletonOf<IMessageRouter>().Use<MessageRouter>();

            ForSingletonOf<UriAliasLookup>().Use<UriAliasLookup>();

            For<IPersistence>().Use<NulloPersistence>();


        }
    }
}
