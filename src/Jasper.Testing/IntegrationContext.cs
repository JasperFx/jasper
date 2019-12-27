using System;
using Jasper.Runtime;
using Jasper.Runtime.Handlers;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Jasper.Testing
{
    public class DefaultApp : IDisposable
    {
        public DefaultApp()
        {
            Host = JasperHost.Basic();
        }

        public IHost Host { get; private set; }

        public void Dispose()
        {
            Host?.Dispose();
            Host = null;
        }

        public void RecycleIfNecessary()
        {
            if (Host == null) Host = JasperHost.Basic();
        }
    }


    public class IntegrationContext : IDisposable, IClassFixture<DefaultApp>
    {
        private readonly DefaultApp _default;

        public IntegrationContext(DefaultApp @default)
        {
            _default = @default;
            _default.RecycleIfNecessary();

            Host = _default.Host;
        }

        public IHost Host { get; private set; }

        public IMessageContext Bus => Host.Get<IMessageContext>();

        public ITransportRuntime Runtime => Host.Get<IMessagingRoot>().Runtime;

        public HandlerGraph Handlers => Host.Get<HandlerGraph>();

        public virtual void Dispose()
        {
            _default.Dispose();

        }


        protected void with(JasperOptions options)
        {
            options.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Host = JasperHost.For(options);
        }

        protected void with(Action<JasperOptions> configuration)
        {
            var registry = new JasperOptions();


            configuration(registry);

            with(registry);
        }

        protected void with<T>() where T : JasperOptions, new()
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
