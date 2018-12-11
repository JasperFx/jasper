using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class DurableCallback : IMessageCallback
    {
        private readonly Envelope _envelope;
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistor _persistor;
        private readonly IWorkerQueue _queue;
        private readonly IRetries _retries;

        public DurableCallback(Envelope envelope, IWorkerQueue queue, IEnvelopePersistor persistor,
            IRetries retries, ITransportLogger logger)
        {
            _envelope = envelope;
            _queue = queue;
            _persistor = persistor;
            _retries = retries;
            _logger = logger;
        }

        public async Task MarkComplete()
        {
            try
            {
                await _persistor.DeleteIncomingEnvelope(_envelope);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                _retries.DeleteIncoming(_envelope);
            }
        }

        public async Task MoveToErrors(Envelope envelope, Exception exception)
        {
            var errorReport = new ErrorReport(envelope, exception);

            try
            {
                await _persistor.MoveToDeadLetterStorage(new[] {errorReport});
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                _retries.LogErrorReport(errorReport);
            }
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

        public async Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.ExecutionTime = time;
            envelope.Status = TransportConstants.Scheduled;

            try
            {
                await _persistor.ScheduleExecution(new[] {envelope});
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                _retries.ScheduleExecution(envelope);
            }
        }
    }
}
