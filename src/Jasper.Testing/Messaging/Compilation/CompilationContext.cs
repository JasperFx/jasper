using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    public abstract class CompilationContext : IDisposable
    {
        public CompilationContext()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private JasperRuntime _runtime;


        protected Envelope theEnvelope;


        public readonly JasperRegistry theRegistry = new JasperRegistry();

        protected void AllHandlersCompileSuccessfully()
        {
            using (var runtime = JasperRuntime.For(theRegistry))
            {
                runtime.Get<HandlerGraph>().Chains.Length.ShouldBeGreaterThan(0);
            }

        }

        public async Task<MessageHandler> HandlerFor<TMessage>()
        {
            if (_runtime == null) _runtime = await JasperRuntime.ForAsync(theRegistry);


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

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
        }
    }
}
