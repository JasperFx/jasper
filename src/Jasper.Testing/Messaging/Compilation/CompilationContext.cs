using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Lamar;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public abstract class CompilationContext: IDisposable
    {
        private Lazy<IContainer> _container;
        private JasperRuntime _runtime;


        protected Envelope theEnvelope;


        public readonly JasperRegistry theRegistry = new JasperRegistry();

        public CompilationContext()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
        }

        [Fact]
        public Task can_compile_all()
        {
            return AllHandlersCompileSuccessfully();
        }

        public async Task AllHandlersCompileSuccessfully()
        {
            var runtime = await JasperRuntime.ForAsync(theRegistry);

            try
            {
                runtime.Get<HandlerGraph>().Chains.Length.ShouldBeGreaterThan(0);
            }
            finally
            {
                await runtime.Shutdown();
            }


        }

        public async Task<MessageHandler> HandlerFor<TMessage>()
        {
            if (_runtime == null)
            {
                _runtime = await JasperRuntime.ForAsync(theRegistry);
            }



            return _runtime.Get<HandlerGraph>().HandlerFor(typeof(TMessage));
        }

        public async Task<IMessageContext> Execute<TMessage>(TMessage message)
        {
            var handler = await HandlerFor<TMessage>();
            theEnvelope = new Envelope(message);
            var context = _runtime.Get<IMessagingRoot>().ContextFor(theEnvelope);

            await handler.Handle(context);

            return context;
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
