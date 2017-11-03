using System;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.WorkerQueues;

namespace Jasper.Bus.Transports.Receiving
{
    // Will be completely independent of any kind of retry or work
    // stealing agent
    public class Listener : IReceiverCallback, IDisposable
    {
        private readonly IPersistence _persistence;
        private readonly IWorkerQueue _workerQueue;
        private readonly CompositeLogger _logger;
        private readonly IListeningAgent _agent;

        public Listener(IPersistence persistence, IWorkerQueue workerQueue, CompositeLogger logger, IListeningAgent agent)
        {
            _persistence = persistence;
            _workerQueue = workerQueue;
            _logger = logger;
            _agent = agent;
        }


        ReceivedStatus IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            // NOTE! We no longer validate against queues not existing.
            // instead, we just shuttle them to the default queue
            try
            {
                _persistence.StoreInitial(messages);

                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;
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
            _persistence.Remove(messages);
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
