using System;
using System.Linq;
using System.Threading;
using Jasper.Conneg;
using Jasper.Conneg.Json;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Persistence;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
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

            this.AddSingleton<IServiceProviderFactory<IServiceCollection>>(new DefaultServiceProviderFactory());

            this.AddLogging();

            For<IMessageLogger>().Use<MessageLogger>().Singleton();
            For<ITransportLogger>().Use<TransportLogger>().Singleton();

            For<IMessageSerializer>().Use<EnvelopeReaderWriter>();
            For<IMessageDeserializer>().Use<EnvelopeReaderWriter>();


            this.AddSingleton(parent.Advanced);
            this.AddSingleton(parent.CodeGeneration);

            For<IHostedService>().Use<DurabilityAgent>();

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
            Policies.Add(new HandlerScopingPolicy(parent.HandlerGraph));

            ForSingletonOf<MessagingSerializationGraph>().Use<MessagingSerializationGraph>();



            For<IEnvelopePersistence>().Use<NulloEnvelopePersistence>();
            this.AddSingleton<InMemorySagaPersistor>();

            this.AddSingleton(parent.HandlerGraph);
            this.AddSingleton<ISubscriberGraph>(new SubscriberGraph());


            For<ITransport>()
                .Use<LoopbackTransport>();

            For<ITransport>()
                .Use<TcpTransport>();

            For<ITransport>()
                .Use<StubTransport>().Singleton();

            Scan(x =>
            {
                x.AssemblyContainingType<JasperServiceRegistry>();
                x.ConnectImplementationsToTypesClosing(typeof(ISerializerFactory<,>));
            });


            ForSingletonOf<IMessagingRoot>().Use<MessagingRoot>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());

            MessagingRootService(x => x.Workers);
            MessagingRootService(x => x.Pipeline);

            MessagingRootService(x => x.Router);
            MessagingRootService(x => x.ScheduledJobs);

            For<IMessageContext>().Use<MessageContext>();



            For<IMessageContext>().Use(c => c.GetInstance<IMessagingRoot>().NewContext());
            For<ICommandBus>().Use(c => c.GetInstance<IMessagingRoot>().NewContext());
            For<IMessagePublisher>().Use(c => c.GetInstance<IMessagingRoot>().NewContext());

            ForSingletonOf<ITransportLogger>().Use<TransportLogger>();

            // I'm not proud of this code, but you need a non-null
            // Container property to use the codegen
            For<IGeneratesCode>().Use(c =>
            {
                var handlers = c.GetInstance<HandlerGraph>();
                handlers.Container = (IContainer)c;

                return handlers;
            });

        }

        public void MessagingRootService<T>(Func<IMessagingRoot, T> expression) where T : class
        {
            For<T>().Use(c => expression(c.GetInstance<IMessagingRoot>())).Singleton();
        }
    }



    internal class HandlerScopingPolicy : IFamilyPolicy
    {
        private readonly HandlerGraph _handlers;

        public HandlerScopingPolicy(HandlerGraph handlers)
        {
            _handlers = handlers;
        }

        private bool matches(Type type)
        {
            var handlerTypes = _handlers.Chains.SelectMany(x => x.Handlers)
                .Select(x => x.HandlerType);

            return handlerTypes.Contains(type);
        }

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsConcrete() && matches(type))
            {
                var instance = new ConstructorInstance(type, type, ServiceLifetime.Scoped);
                return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
            }

            return null;
        }
    }
}
