using System;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.WorkerQueues;

namespace Jasper.Bus.Transports.Receiving
{
    public class LightweightListener : IListener
    {
        private readonly IWorkerQueue _workerQueue;
        private readonly CompositeLogger _logger;
        private readonly IListeningAgent _agent;

        public LightweightListener(IWorkerQueue workerQueue, CompositeLogger logger, IListeningAgent agent)
        {
            _workerQueue = workerQueue;
            _logger = logger;
            _agent = agent;
        }

        ReceivedStatus IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            try
            {
                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;

                    message.Callback = new LightweightCallback(_workerQueue);
                    _workerQueue.Enqueue(message);
                }

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }

        void IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            // Nothing
        }

        void IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            // Nothing
        }

        void IReceiverCallback.Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
        }

        public void Dispose()
        {
            _agent?.Dispose();
        }

        public void Start()
        {
            _agent.Start(this);
        }
    }
}
