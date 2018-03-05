using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Lamar;
using Xunit;

namespace Jasper.Testing.Messaging.Compilation
{
    [Collection("compilation")]
    public abstract class CompilationContext: IDisposable
    {
        private Lazy<IContainer> _container;


        protected Envelope theEnvelope;


        public readonly JasperRegistry theRegistry = new JasperRegistry();
        private Lazy<JasperRuntime> _runtime;

        public CompilationContext()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
            _runtime = new Lazy<JasperRuntime>(() =>
            {
                return JasperRuntime.For(theRegistry);
            });
        }



        public IContainer Container => _container.Value;

        public HandlerGraph Graph => _runtime.Value.Get<HandlerGraph>();

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
        }

        public void AllHandlersCompileSuccessfully()
        {
            Graph.Chains.Length.ShouldBeGreaterThan(0);
        }

        public MessageHandler HandlerFor<TMessage>()
        {
            return Graph.HandlerFor(typeof(TMessage));
        }

        public async Task<IMessageContext> Execute<TMessage>(TMessage message)
        {
            var handler = HandlerFor<TMessage>();
            theEnvelope = new Envelope(message);
            var context = _runtime.Value.Get<IMessagingRoot>().ContextFor(theEnvelope);

            await handler.Handle(context);

            return context;
        }

        public void Dispose()
        {
            if (_runtime.IsValueCreated)
            {
                _runtime.Value.Dispose();
            }
        }
    }
}
