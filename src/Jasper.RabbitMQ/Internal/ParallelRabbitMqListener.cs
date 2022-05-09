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
    public class ParallelRabbitMqListener : IListener
    {
        private readonly IList<RabbitMqListener> _listeners = new List<RabbitMqListener>();

        public ParallelRabbitMqListener(ILogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport)
        {
            Address = endpoint.Uri;
            for (var i = 0; i < endpoint.ListenerCount; i++)
            {
                var listener = new RabbitMqListener(logger, endpoint, transport);
                _listeners.Add(listener);
            }
        }

        public void Dispose()
        {
            foreach (var listener in _listeners) listener.SafeDispose();
        }

        public Uri Address { get; }


        public ListeningStatus Status
        {
            get => _listeners[0].Status;
            set
            {
                foreach (var listener in _listeners) listener.Status = value;
            }
        }

        public void Start(IListeningWorkerQueue? callback, CancellationToken cancellation)
        {
            foreach (var listener in _listeners) listener.Start(callback, cancellation);
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
