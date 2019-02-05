using System;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Xunit;

namespace MessagingTests
{
    public class DefaultApp : IDisposable
    {
        public DefaultApp()
        {
            Host = JasperHost.Basic();
        }

        public IJasperHost Host { get; private set; }

        public void Dispose()
        {
            Host?.Dispose();
        }

        public void RecycleIfNecessary()
        {
            if (Host.IsDisposed)
            {
                Host = JasperHost.Basic();
            }
        }
    }


    public class IntegrationContext : IDisposable, IClassFixture<DefaultApp>
    {
        private DefaultApp _default;

        public IntegrationContext(DefaultApp @default)
        {
            _default = @default;
            _default.RecycleIfNecessary();

            Host = _default.Host;


        }

        public IJasperHost Host { get; private set; }

        public IMessageContext Bus => Host.Get<IMessageContext>();

        public ISubscriberGraph Subscribers => Host.Get<ISubscriberGraph>();

        public HandlerGraph Handlers => Host.Get<HandlerGraph>();

        public virtual void Dispose()
        {
            Host?.Dispose();
        }


        protected void with(JasperRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Host = JasperHost.For(registry);
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
