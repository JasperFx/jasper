using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
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
            _host?.Dispose();
        }

        private IHost _host;


        protected Envelope theEnvelope;


        public readonly JasperRegistry theRegistry = new JasperRegistry();

        protected void AllHandlersCompileSuccessfully()
        {
            using (var runtime = JasperHost.For(theRegistry))
            {
                runtime.Get<HandlerGraph>().Chains.Length.ShouldBeGreaterThan(0);
            }

        }

        public MessageHandler HandlerFor<TMessage>()
        {
            if (_host == null) _host = JasperHost.For(theRegistry);


            return _host.Get<HandlerGraph>().HandlerFor(typeof(TMessage));
        }

        public async Task<IMessageContext> Execute<TMessage>(TMessage message)
        {
            var handler = HandlerFor<TMessage>();
            theEnvelope = new Envelope(message);
            var context = _host.Get<IMessagingRoot>().ContextFor(theEnvelope);

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
