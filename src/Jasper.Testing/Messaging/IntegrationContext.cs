using System;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports.Configuration;
using Xunit;

namespace Jasper.Testing.Messaging
{
    [Collection("integration")]
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }

        public IMessageContext Bus => Runtime.Get<IMessageContext>();

        public IChannelGraph Channels => Runtime.Get<IChannelGraph>();

        public MessagingSettings MessagingSettings => Runtime.Get<MessagingSettings>();

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
