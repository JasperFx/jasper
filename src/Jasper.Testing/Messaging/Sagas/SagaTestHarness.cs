using System;
using System.Threading.Tasks;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;

namespace Jasper.Testing.Messaging.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private MessageHistory _history;
        private JasperRuntime _runtime;

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        protected async Task withApplication()
        {
            _runtime = await JasperRuntime.ForAsync(_ =>
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

        protected virtual void configure(JasperRegistry registry)
        {
            // nothing
        }

        protected async Task invoke<T>(T message)
        {
            if (_history == null) await withApplication();

            await _runtime.Messaging.Invoke(message);
        }

        protected async Task send<T>(T message)
        {
            if (_history == null) await withApplication();

            await _history.WatchAsync(() => _runtime.Messaging.Send(message), 10000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _history.WatchAsync(() => _runtime.Messaging.Send(message, e => e.SagaId = sagaId.ToString()),
                10000);
        }

        protected TSagaState LoadState(object id)
        {
            return _runtime.Get<InMemorySagaPersistor>().Load<TSagaState>(id);
        }
    }

    public static class HandlerConfigurationExtensions
    {
        public static IHandlerConfiguration DisableConventionalDiscovery(this IHandlerConfiguration handlers,
            bool disabled = true)
        {
            if (disabled) handlers.Discovery(x => x.DisableConventionalDiscovery());

            return handlers;
        }

        public static IHandlerConfiguration OnlyType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<T>();
            });

            return handlers;
        }

        public static IHandlerConfiguration IncludeType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x => x.IncludeType<T>());

            return handlers;
        }
    }
}
