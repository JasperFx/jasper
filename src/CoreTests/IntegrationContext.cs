using System;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Xunit;

namespace CoreTests
{
    [Collection("integration")]
    public class IntegrationContext : IDisposable
    {
        public IJasperHost Runtime { get; private set; }

        public IMessageContext Bus => Runtime.Get<IMessageContext>();

        public ISubscriberGraph Subscribers => Runtime.Get<ISubscriberGraph>();

        public HandlerGraph Handlers => Runtime.Get<HandlerGraph>();

        public virtual void Dispose()
        {
            Runtime?.Dispose();
        }


        protected void withAllDefaults()
        {
            with(new JasperRegistry());
        }

        protected void with(JasperRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperHost.For(registry);
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

        protected HandlerChain chainFor<T>()
        {
            return Handlers.ChainFor<T>();
        }
    }
}
