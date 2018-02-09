using System;
using System.Collections.ObjectModel;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Transports.Configuration;
using Xunit;

namespace Jasper.Testing.Bus
{
    [Collection("integration")]
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }

        public IServiceBus Bus => Runtime.Get<IServiceBus>();

        public IChannelGraph Channels => Runtime.Get<IChannelGraph>();

        public BusSettings BusSettings => Runtime.Get<BusSettings>();

        protected void withAllDefaults()
        {
            with(new JasperRegistry());
            Handlers = Runtime.Get<HandlerGraph>();
        }

        protected void with(JasperRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperRuntime.For(registry);


            Handlers = Runtime.Get<HandlerGraph>();
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
