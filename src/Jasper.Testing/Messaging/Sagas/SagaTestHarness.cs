using System;
using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;


namespace Jasper.Testing.Messaging.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private readonly MessageHistory _history;
        private readonly JasperRuntime _runtime;

        public SagaTestHarness()
        {
            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                _.Include<MessageTrackingExtension>();

                _.Publish.AllMessagesTo(TransportConstants.LoopbackUri);

                configure(_);
            });

            _history = _runtime.Get<MessageHistory>();

        }

        protected string codeFor<T>()
        {
            return _runtime.Get<HandlerGraph>().HandlerFor<T>().Chain.SourceCode;
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        protected virtual void configure(JasperRegistry registry)
        {
            // nothing
        }

        protected Task send<T>(T message)
        {
            return _history.WatchAsync(() => _runtime.Messaging.Send(message), 10000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _history.WatchAsync(() => _runtime.Messaging.Send(message, e => e.SagaId = sagaId.ToString()), 10000);
        }

        protected TSagaState LoadState(object id)
        {
            return _runtime.Get<InMemorySagaPersistor>().Load<TSagaState>(id);
        }
    }
}
