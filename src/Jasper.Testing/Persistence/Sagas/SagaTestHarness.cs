using System;
using System.Threading.Tasks;
using Jasper.Persistence.Sagas;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Jasper.Transports;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Testing.Persistence.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private IHost _host;

        public void Dispose()
        {
            _host?.Dispose();
        }

        protected void withApplication()
        {
            _host = JasperHost.For(opts =>
            {
                opts.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                opts.PublishAllMessages().To(TransportConstants.LocalUri);

                configure(opts);
            });

        }

        protected string codeFor<T>()
        {
            return _host.Get<HandlerGraph>().HandlerFor<T>().Chain.SourceCode;
        }

        protected virtual void configure(JasperOptions options)
        {
            // nothing
        }

        protected async Task invoke<T>(T message)
        {
            if (_host == null) withApplication();

            await _host.Get<IMessagePublisher>().InvokeAsync(message);
        }

        protected async Task send<T>(T message)
        {
            if (_host == null) withApplication();

            await _host.ExecuteAndWaitAsync(x => x.SendAsync(message));
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _host.ExecuteAndWaitAsync(x =>
            {
                var envelope = new Envelope(message)
                {
                    SagaId = sagaId.ToString()
                };

                return x.SendEnvelopeAsync(envelope);
            }, 10000);
        }

        protected TSagaState LoadState(object id)
        {
            return _host.Get<InMemorySagaPersistor>().Load<TSagaState>(id);
        }
    }

}
