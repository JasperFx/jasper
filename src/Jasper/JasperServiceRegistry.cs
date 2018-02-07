using BlueMilk;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Http.Routing;
using Jasper.Http.Transport;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Jasper
{
    internal class JasperServiceRegistry : ServiceRegistry
    {
        public JasperServiceRegistry(JasperRegistry parent)
        {
            Policies.OnMissingFamily<LoggerPolicy>();

            conneg(parent);
            messaging(parent);

            routing(parent);
        }

        private void routing(JasperRegistry parent)
        {
            this.AddScoped<IHttpContextAccessor>(x => new HttpContextAccessor());
            this.AddSingleton(parent.Http.Routes);
            ForSingletonOf<IUrlRegistry>().Use(parent.Http.Routes.Router.Urls);
            For<IServer>().Use<NulloServer>();
        }

        private void conneg(JasperRegistry parent)
        {
            this.AddOptions();

            var forwarding = new Forwarders();
            For<Forwarders>().Use(forwarding);

            Scan(_ =>
            {
                _.Assembly(parent.ApplicationAssembly);
                _.AddAllTypesOf<IMessageSerializer>();
                _.AddAllTypesOf<IMessageDeserializer>();
                _.With(new ForwardingRegistration(forwarding));
            });
        }

        private void messaging(JasperRegistry parent)
        {
            this.AddSingleton(parent.Bus.Graph);
            this.AddSingleton<IChannelGraph>(parent.Bus.Channels);
            this.AddSingleton<ILocalWorkerSender>(parent.Bus.LocalWorker);

            ForSingletonOf<IScheduledJobProcessor>().UseIfNone<InMemoryScheduledJobProcessor>();


            ForSingletonOf<ITransport>()
                .Use<LoopbackTransport>();

            ForSingletonOf<ITransport>()
                .Use<TcpTransport>();

            ForSingletonOf<ITransport>()
                .Use<HttpTransport>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());

            ForSingletonOf<IWorkerQueue>().Use<WorkerQueue>();

            For<IServiceBus>().Use<ServiceBus>();
            ForSingletonOf<IHandlerPipeline>().Use<HandlerPipeline>();

            ForSingletonOf<CompositeMessageLogger>().Use<CompositeMessageLogger>();
            ForSingletonOf<CompositeTransportLogger>().Use<CompositeTransportLogger>();

            ForSingletonOf<INodeDiscovery>().UseIfNone<InMemoryNodeDiscovery>();
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone<InMemorySubscriptionsRepository>();

            ForSingletonOf<IReplyWatcher>().Use(new ReplyWatcher());

            For<IUriLookup>().Use<ConfigUriLookup>();

            ForSingletonOf<BusMessageSerializationGraph>().Use<BusMessageSerializationGraph>();

            ForSingletonOf<IMessageRouter>().Use<MessageRouter>();

            ForSingletonOf<UriAliasLookup>().Use<UriAliasLookup>();


            For<IPersistence>().Use<NulloPersistence>();

            For<IEnvironmentRecorder>().Use<EnvironmentRecorder>();
        }
    }
}
