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
            with(new JasperRegistry());
            Handlers = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(JasperRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperRuntime.For(registry);


            Handlers = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(Action<JasperRegistry> configuration)
        {
            var registry = new JasperRegistry();


            configuration(registry);

            with(registry);
        }

        protected void with<T>() where T : JasperRegistry, new()
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
