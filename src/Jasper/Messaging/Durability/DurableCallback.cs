using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class DurableCallback : IMessageCallback
    {
        private readonly Envelope _envelope;
        private readonly IEnvelopePersistor _persistor;
        private readonly IWorkerQueue _queue;
        private readonly IRetries _retries;

        public DurableCallback(Envelope envelope, IWorkerQueue queue, IEnvelopePersistor persistor,
            IRetries retries)
        {
            _envelope = envelope;
            _queue = queue;
            _persistor = persistor;
            _retries = retries;
        }

        public Task MarkComplete()
        {
            _retries.DeleteIncoming(_envelope);

            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            _retries.LogErrorReport(new ErrorReport(envelope, exception));
            return Task.CompletedTask;
        }

        public async Task Requeue(Envelope envelope)
        {
            try
            {
                envelope.Attempts++;
                await _persistor.IncrementIncomingEnvelopeAttempts(envelope);
            }
            catch (Exception)
            {
                // Not going to worry about a failure here
            }

            await _queue.Enqueue(envelope);
        }

        public Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            envelope.ExecutionTime = time;
            envelope.Status = TransportConstants.Scheduled;
            _retries.ScheduleExecution(envelope);


            return Task.CompletedTask;
        }
    }
}
