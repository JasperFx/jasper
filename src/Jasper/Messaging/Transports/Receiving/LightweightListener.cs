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
        private readonly IListeningAgent _agent;
        private readonly ITransportLogger _logger;
        private readonly IWorkerQueue _workerQueue;

        public LightweightListener(IWorkerQueue workerQueue, ITransportLogger logger, IListeningAgent agent)
        {
            _workerQueue = workerQueue;
            _logger = logger;
            _agent = agent;
        }

        public ListeningStatus Status
        {
            get => _agent.Status;
            set => _agent.Status = value;
        }

        public Uri Address => _agent.Address;

        async Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return await ProcessReceivedMessages(uri, messages, now);
        }

        private async Task<ReceivedStatus> ProcessReceivedMessages(Uri uri, Envelope[] messages, DateTime now)
        {
            try
            {
                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;

                    message.Callback = new LightweightCallback(_workerQueue);

                    if (message.IsDelayed(now))
                        _workerQueue.ScheduledJobs.Enqueue(message.ExecutionTime.Value, message);
                    else
                        await _workerQueue.Enqueue(message);
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
