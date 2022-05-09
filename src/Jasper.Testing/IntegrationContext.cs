using System;
using Jasper.Runtime;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Testing
{
    public class DefaultApp : IDisposable
    {
        public DefaultApp()
        {
            Host = JasperHost.For(x =>
            {
                x.Handlers.IncludeType<MessageConsumer>();
            });
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

        public HandlerChain ChainFor<T>()
        {
            return Host.Get<HandlerGraph>().ChainFor<T>();
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

        public IExecutionContext Publisher => Host.Get<IExecutionContext>();
        public ICommandBus Bus => Host.Get<ICommandBus>();

        public HandlerGraph Handlers => Host.Get<HandlerGraph>();

        public virtual void Dispose()
        {
            _default.Dispose();

        }


        protected void with(Action<JasperOptions> configuration)
        {
            Host = JasperHost.For(opts =>
            {
                configuration(opts);
                opts.Services.Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
            });
        }

        protected HandlerChain chainFor<T>()
        {
            return Handlers.ChainFor<T>();
        }
    }
}
