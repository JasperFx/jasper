using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Testing.Messaging.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private MessageHistory _history;
        private IHost _host;

        public void Dispose()
        {
            _host?.Dispose();
        }

        protected void withApplication()
        {
            _host = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                _.Include<MessageTrackingExtension>();

                _.Publish.AllMessagesTo(TransportConstants.LoopbackUri);

                configure(_);
            });

            _history = _host.Get<MessageHistory>();
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
            if (_history == null) withApplication();

            await _host.Get<IMessagePublisher>().Invoke(message);
        }

        protected async Task send<T>(T message)
        {
            if (_history == null) withApplication();

            await _history.WatchAsync(() => _host.Get<IMessagePublisher>().Send(message), 60000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _history.WatchAsync(() => _host.Get<IMessagePublisher>().Send(message, e => e.SagaId = sagaId.ToString()),
                10000);
        }

        protected TSagaState LoadState(object id)
        {
            return _host.Get<InMemorySagaPersistor>().Load<TSagaState>(id);
        }
    }

}
