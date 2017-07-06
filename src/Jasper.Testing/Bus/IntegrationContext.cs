using System;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;

namespace Jasper.Testing.Bus
{
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }

        public IServiceBus Bus => Runtime.Container.GetInstance<IServiceBus>();

        public ChannelGraph Channels => Runtime.Container.GetInstance<ChannelGraph>();

        protected void withAllDefaults()
        {
            with(new JasperBusRegistry());
            Handlers = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(JasperBusRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperRuntime.For(registry);


            Handlers = Runtime.Container.GetInstance<HandlerGraph>();
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
            return Handlers.ChainFor<T>();
        }

        public HandlerGraph Handlers { get; private set; }
    }
}
