using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.WorkerQueues;
using Jasper.EnvironmentChecks;
using Jasper.Http.Transport;
using Jasper.Internals;
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
                .Use<TcpTransport>();

            ForSingletonOf<ITransport>()
                .Use<HttpTransport>();

            ForSingletonOf<ObjectPoolProvider>().Use<DefaultObjectPoolProvider>();

            ForSingletonOf<IWorkerQueue>().Use<WorkerQueue>();

            For<IServiceBus>().Use<ServiceBus>();
            ForSingletonOf<IHandlerPipeline>().Use<HandlerPipeline>();

            ForSingletonOf<CompositeLogger>().Use<CompositeLogger>();
            ForSingletonOf<CompositeTransportLogger>().Use<CompositeTransportLogger>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();

            For<IUriLookup>().Use<ConfigUriLookup>();

            ForSingletonOf<BusMessageSerializationGraph>().Use<BusMessageSerializationGraph>();

            ForSingletonOf<IMessageRouter>().Use<MessageRouter>();

            ForSingletonOf<UriAliasLookup>().Use<UriAliasLookup>();


            For<IPersistence>().Use<NulloPersistence>();

            For<IEnvironmentRecorder>().Use<EnvironmentRecorder>();
        }
    }
}
