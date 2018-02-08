using BlueMilk;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Http.ContentHandling;
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
            this.AddSingleton<ConnegRules>();
            this.AddSingleton<IServer, NulloServer>();

            this.AddScoped<IHttpContextAccessor>(x => new HttpContextAccessor());
            this.AddSingleton(parent.Http.Routes.Router);
            this.AddSingleton(parent.Http.Routes);
            ForSingletonOf<IUrlRegistry>().Use(parent.Http.Routes.Router.Urls);
            For<IServer>().Use<NulloServer>();

            Policies.OnMissingFamily<LoggerPolicy>();
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


            For<ITransport>()
                .Use<LoopbackTransport>();

            For<ITransport>()
                .Use<TcpTransport>();

            For<ITransport>()
                .Use<HttpTransport>();

            ForSingletonOf<MessagingRoot>().Use<MessagingRoot>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());

            this.AddSingleton<IWorkerQueue>(s => s.GetService<MessagingRoot>().Workers);
            this.AddSingleton(s => s.GetService<MessagingRoot>().Pipeline);
            this.AddSingleton(s => s.GetService<MessagingRoot>().Serialization);
            this.AddTransient(s => s.GetService<MessagingRoot>().Build());
            this.AddSingleton(s => s.GetService<MessagingRoot>().Logger);
            this.AddSingleton(s => s.GetService<MessagingRoot>().Router);
            this.AddSingleton(s => s.GetService<MessagingRoot>().Lookup);
            this.AddSingleton(s => s.GetService<MessagingRoot>().ScheduledJobs);


            ForSingletonOf<CompositeTransportLogger>().Use<CompositeTransportLogger>();

            ForSingletonOf<INodeDiscovery>().UseIfNone(new InMemoryNodeDiscovery(parent.BusSettings));
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone(new InMemorySubscriptionsRepository());


            For<IUriLookup>().Use<ConfigUriLookup>();


            For<IPersistence>().Use<NulloPersistence>();

            For<IEnvironmentRecorder>().Use<EnvironmentRecorder>();
        }
    }
}
