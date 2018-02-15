using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Transports.Receiving
{
    public class LightweightListener : IListener
    {
        private readonly IWorkerQueue _workerQueue;
        private readonly CompositeTransportLogger _logger;
        private readonly IListeningAgent _agent;

        public LightweightListener(IWorkerQueue workerQueue, CompositeTransportLogger logger, IListeningAgent agent)
        {
            _workerQueue = workerQueue;
            _logger = logger;
            _agent = agent;
        }

        public Uri Address => _agent.Address;

        async Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            try
            {
                var now = DateTime.UtcNow;

                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;

                    message.Callback = new LightweightCallback(_workerQueue);

                    if (message.IsDelayed(now))
                    {
                        _workerQueue.ScheduledJobs.Enqueue(message.ExecutionTime.Value, message);
                    }
                    else
                    {
                        await _workerQueue.Enqueue(message);
                    }
                }

                _logger.IncomingBatchReceived(messages);

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }

        Task IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        Task IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        Task IReceiverCallback.Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
            return Task.CompletedTask;
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
