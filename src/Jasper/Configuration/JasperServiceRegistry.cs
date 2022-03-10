using System;
using System.Linq;
using Jasper.Logging;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Sagas;
using Jasper.Runtime;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Configuration
{
    internal class JasperServiceRegistry : ServiceRegistry
    {
        public JasperServiceRegistry(JasperOptions parent)
        {
            For<IMetrics>().Use<NulloMetrics>();
            //For<IHostedService>().Use<MetricsCollector>();

            this.AddSingleton<IServiceProviderFactory<IServiceCollection>>(new DefaultServiceProviderFactory());

            this.AddLogging();

            For<IMessageLogger>().Use<MessageLogger>().Singleton();
            For<ITransportLogger>().Use<TransportLogger>().Singleton();

            this.AddSingleton(parent.Advanced);

            this.AddOptions();

            var forwarding = new Forwarders();
            For<Forwarders>().Use(forwarding);

            Scan(_ =>
            {
                _.Assembly(parent.ApplicationAssembly);
                _.With(new ForwardingRegistration(forwarding));
            });

            Policies.Add(new HandlerScopingPolicy(parent.HandlerGraph));

            For<IEnvelopePersistence>().Use<NulloEnvelopePersistence>();
            this.AddSingleton<InMemorySagaPersistor>();

            this.AddSingleton(parent.HandlerGraph);

            ForSingletonOf<IMessagingRoot>().Use<MessagingRoot>();

            ForSingletonOf<ObjectPoolProvider>().Use(new DefaultObjectPoolProvider());

            MessagingRootService(x => x.Pipeline);

            MessagingRootService(x => x.Router);
            MessagingRootService(x => x.ScheduledJobs);
            MessagingRootService(x => x.Runtime);

            For<IExecutionContext>().Use(c => c.GetInstance<IMessagingRoot>().NewContext()).Scoped();
            For<ICommandBus>().Use<CommandBus>().Scoped();
            For<IMessagePublisher>().Use<MessagePublisher>().Scoped();


            // I'm not proud of this code, but you need a non-null
            // Container property to use the codegen
            For<ICodeFileCollection>().Use(c =>
            {
                var handlers = c.GetInstance<HandlerGraph>();
                handlers.Container = (IContainer) c;

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

        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.IsConcrete() && matches(type))
            {
                var instance = new ConstructorInstance(type, type, ServiceLifetime.Scoped);
                return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
            }

            return null;
        }

        private bool matches(Type type)
        {
            var handlerTypes = _handlers.Chains.SelectMany(x => x.Handlers)
                .Select(x => x.HandlerType);

            return handlerTypes.Contains(type);
        }
    }
}
