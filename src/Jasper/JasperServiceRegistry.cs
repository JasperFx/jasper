using BlueMilk;
using Jasper.Conneg;
using Jasper.EnvironmentChecks;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Jasper
{
    internal class JasperServiceRegistry : ServiceRegistry
    {
        public JasperServiceRegistry(JasperRegistry parent)
        {
            // Will be overwritten when ASP.Net is in place too,
            // but that's okay
            this.AddSingleton<HostedServiceExecutor>();

            conneg(parent);
            messaging(parent);
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
            this.AddSingleton(parent.Messaging.Graph);
            this.AddSingleton<IChannelGraph>(parent.Messaging.Channels);
            this.AddSingleton<ILocalWorkerSender>(parent.Messaging.LocalWorker);


            For<ITransport>()
                .Use<LoopbackTransport>();

            For<ITransport>()
                .Use<TcpTransport>();



            ForSingletonOf<IMessagingRoot>().Use<MessagingRoot>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());

            this.AddSingleton<IWorkerQueue>(s => s.GetService<MessagingRoot>().Workers);
            this.AddSingleton(s => s.GetService<IMessagingRoot>().Pipeline);
            this.AddSingleton(s => s.GetService<IMessagingRoot>().Serialization);
            this.AddTransient(s => s.GetService<IMessagingRoot>().NewContext());
            this.AddSingleton(s => s.GetService<IMessagingRoot>().Logger);
            this.AddSingleton(s => s.GetService<IMessagingRoot>().Router);
            this.AddSingleton(s => s.GetService<IMessagingRoot>().Lookup);
            this.AddSingleton(s => s.GetService<IMessagingRoot>().ScheduledJobs);


            ForSingletonOf<ITransportLogger>().Use<CompositeTransportLogger>();

            ForSingletonOf<INodeDiscovery>().UseIfNone(new InMemoryNodeDiscovery(parent.MessagingSettings));
            ForSingletonOf<ISubscriptionsRepository>().UseIfNone(new InMemorySubscriptionsRepository());


            For<IUriLookup>().Use<ConfigUriLookup>();

            For<IEnvironmentRecorder>().Use<EnvironmentRecorder>();
        }
    }
}
