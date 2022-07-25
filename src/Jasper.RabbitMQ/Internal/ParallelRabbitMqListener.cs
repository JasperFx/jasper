using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.RabbitMQ.Internal
{
    public class ParallelRabbitMqListener : IListener, IDisposable
    {
        private readonly IList<RabbitMqListener> _listeners = new List<RabbitMqListener>();
        private IReceiver? _callback;
        private CancellationToken _cancellation;

        public ParallelRabbitMqListener(ILogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport, IReceiver receiver)
        {
            Address = endpoint.Uri;
            for (var i = 0; i < endpoint.ListenerCount; i++)
            {
                var listener = new RabbitMqListener(logger, endpoint, transport, receiver);
                _listeners.Add(listener);
            }
        }

        public void Dispose()
        {
            foreach (var listener in _listeners) listener.SafeDispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public Uri Address { get; }


        public ListeningStatus Status => _listeners[0].Status;

        public async ValueTask StopAsync()
        {
            foreach (var listener in _listeners)
            {
                await listener.StopAsync();
            }
        }

        public async ValueTask RestartAsync()
        {
            foreach (var listener in _listeners)
            {
                await listener.RestartAsync();
            }
        }

        public void Start(IReceiver callback, CancellationToken cancellation)
        {
            _callback = callback;
            _cancellation = cancellation;
            foreach (var listener in _listeners)
            {
                listener.Start(callback, cancellation);
            }
        }

        public Task<bool> TryRequeueAsync(Envelope envelope)
        {
            var listener = _listeners.FirstOrDefault();
            return listener != null ? listener.TryRequeueAsync(envelope) : Task.FromResult(false);
        }

        public ValueTask CompleteAsync(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.CompleteAsync(envelope);
        }

        public ValueTask DeferAsync(Envelope envelope)
        {
            return RabbitMqChannelCallback.Instance.DeferAsync(envelope);
        }
    }
}
