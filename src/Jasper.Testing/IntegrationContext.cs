using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Xunit;

namespace Jasper.Testing
{
    [Collection("integration")]
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }

        public IMessageContext Bus => Runtime.Get<IMessageContext>();

        public IChannelGraph Channels => Runtime.Get<IChannelGraph>();


        protected Task withAllDefaults()
        {
            return with(new JasperRegistry());
        }

        protected async Task with(JasperRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = await JasperRuntime.ForAsync(registry);
        }

        protected Task with(Action<JasperRegistry> configuration)
        {
            var registry = new JasperRegistry();


            configuration(registry);

            return with(registry);
        }

        protected Task with<T>() where T : JasperRegistry, new()
        {
            var registry = new T();
            return with(registry);
        }

        public virtual void Dispose()
        {
            Runtime?.Dispose();
        }

        protected HandlerChain chainFor<T>()
        {
            return Handlers.ChainFor<T>();
        }

        public HandlerGraph Handlers => Runtime.Get<HandlerGraph>();
    }
}
