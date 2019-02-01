using System;
using System.Linq.Expressions;
using Jasper.Conneg;
using Jasper.EnvironmentChecks;
using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util.Lamar;
using Lamar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Configuration
{
    internal class JasperServiceRegistry : ServiceRegistry
    {
        public JasperServiceRegistry(JasperRegistry parent)
        {
            For<IMetrics>().Use<NulloMetrics>();
            For<IHostedService>().Use<MetricsCollector>();

            this.AddLogging();

            For<IMessageLogger>().Use<MessageLogger>().Singleton();
            For<ITransportLogger>().Use<TransportLogger>().Singleton();


            this.AddSingleton(parent.CodeGeneration);

            For<IHostedService>().Use<BackPressureAgent>();

            Policies.Add(new LoggerPolicy());
            Policies.Add(new OptionsPolicy());

            conneg(parent);
            messaging(parent);

            aspnetcore(parent);
        }

        private void aspnetcore(JasperRegistry parent)
        {
            this.AddSingleton<ConnegRules>();

            this.AddScoped<IHttpContextAccessor>(x => new HttpContextAccessor());
            this.AddSingleton(parent.HttpRoutes.Routes);
            ForSingletonOf<IUrlRegistry>().Use<UrlGraph>();

            this.AddSingleton<IServiceProviderFactory<IServiceCollection>>(new DefaultServiceProviderFactory());


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
            ForSingletonOf<MessagingSerializationGraph>().Use<MessagingSerializationGraph>();

            For<IEnvelopePersistor>().Use<NulloEnvelopePersistor>();
            this.AddSingleton<InMemorySagaPersistor>();

            this.AddSingleton(parent.Messaging.Graph);
            this.AddSingleton<ISubscriberGraph>(parent.Messaging.Subscribers);
            this.AddSingleton<ILocalWorkerSender>(parent.Messaging.LocalWorker);

            this.AddSingleton<IRetries, EnvelopeRetries>();

            For<ITransport>()
                .Use<LoopbackTransport>();

            For<ITransport>()
                .Use<TcpTransport>();


            ForSingletonOf<IMessagingRoot>().Use<MessagingRoot>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());


            MessagingRootService(x => x.Workers);
            MessagingRootService(x => x.Pipeline);

            MessagingRootService(x => x.Router);
            MessagingRootService(x => x.ScheduledJobs);

            For<IMessageContext>().Use(new MessageContextInstance(typeof(IMessageContext)));
            For<IMessageContext>().Use(new MessageContextInstance(typeof(ICommandBus)));
            For<IMessageContext>().Use(new MessageContextInstance(typeof(IMessagePublisher)));

            ForSingletonOf<ITransportLogger>().Use<TransportLogger>();

            For<IEnvironmentRecorder>().Use<EnvironmentRecorder>();
        }

        public void MessagingRootService<T>(Expression<Func<IMessagingRoot, T>> expression) where T : class
        {
            For<T>().Use(new MessagingRootInstance<T>(expression));
        }
    }
}
