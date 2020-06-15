using System;
using System.Collections.Generic;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.RabbitMQ.Internal
{
    public class ParallelRabbitMqListener : IListener
    {
        private readonly IList<RabbitMqListener> _listeners = new List<RabbitMqListener>();

        public ParallelRabbitMqListener(ITransportLogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport)
        {
            Address = endpoint.Uri;
            for (int i = 0; i < endpoint.ListenerCount; i++)
            {
                var listener = new RabbitMqListener(logger, endpoint, transport);
                _listeners.Add(listener);
            }
        }

        public void Dispose()
        {
            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }
        }

        public Uri Address { get; }



        public ListeningStatus Status
        {
            get => _listeners[0].Status;
            set
            {
                foreach (var listener in _listeners)
                {
                    listener.Status = value;
                }
            }
        }

        public void Start(IListeningWorkerQueue callback)
        {
            foreach (var listener in _listeners)
            {
                listener.Start(callback);
            }
        }

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            foreach (var listener in _listeners)
            {
                listener.StartHandlingInline(pipeline);
            }
        }
    }
}
