using System;
using Jasper;
using JasperBus.Model;

namespace JasperBus.Tests
{
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }

        protected void withAllDefaults()
        {
            with(new JasperBusRegistry());
            Graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(JasperBusRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperRuntime.For(registry);


            Graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(Action<JasperBusRegistry> configuration)
        {
            var registry = new JasperBusRegistry();


            configuration(registry);

            with(registry);
        }

        protected void with<T>() where T : JasperBusRegistry, new()
        {
            var registry = new T();
            with(registry);
        }

        public virtual void Dispose()
        {
            Runtime?.Dispose();
        }

        protected HandlerChain chainFor<T>()
        {
            return Graph.ChainFor<T>();
        }

        public HandlerGraph Graph { get; private set; }
    }
}
