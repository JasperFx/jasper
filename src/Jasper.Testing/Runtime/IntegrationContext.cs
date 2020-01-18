using System;
using Jasper.Runtime.Handlers;
using Lamar;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Runtime
{
    public class DefaultApp : IDisposable
    {
        public DefaultApp()
        {
            Host = JasperHost.Basic();
        }

        public IHost Host { get; private set; }

        public IContainer Container => Host.Get<IContainer>();

        public void Dispose()
        {
            Host?.Dispose();
            Host = null;
        }

        public void RecycleIfNecessary()
        {
            if (Host == null)
            {
                Host = JasperHost.Basic();
            }
        }

        public HandlerChain ChainFor<T>()
        {
            return Host.Get<HandlerGraph>().ChainFor<T>();
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

        public IContainer Container => Host.Get<IContainer>();

        public IHost Host { get; private set; }

        public IMessageContext Bus => Host.Get<IMessageContext>();


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
