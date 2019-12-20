using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Polly;
using Polly.Retry;

namespace Jasper.Messaging.Durability
{
    public class DurableCallback : IMessageCallback
    {
        private readonly Envelope _envelope;
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistence _persistence;
        private readonly IWorkerQueue _queue;
        private readonly AsyncRetryPolicy _policy;

        public DurableCallback(Envelope envelope, IWorkerQueue queue, IEnvelopePersistence persistence,
            ITransportLogger logger)
        {
            _envelope = envelope;
            _queue = queue;
            _persistence = persistence;
            _logger = logger;

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => (i*100).Milliseconds()
                    , (e, timeSpan) => {
                        _logger.LogException(e);
                    });

        }

        public Task MarkComplete()
        {
            return _policy.ExecuteAsync(() => _persistence.DeleteIncomingEnvelope(_envelope));
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            var errorReport = new ErrorReport(envelope, exception);

            return _policy.ExecuteAsync(() => _persistence.MoveToDeadLetterStorage(new[] {errorReport}));
        }

        public async Task Requeue(Envelope envelope)
        {
            envelope.Attempts++;

            await _queue.Enqueue(envelope);

            await _policy.ExecuteAsync(() => _persistence.IncrementIncomingEnvelopeAttempts(envelope));
        }

        public Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.ExecutionTime = time;
            envelope.Status = EnvelopeStatus.Scheduled;

            return _policy.ExecuteAsync(() => _persistence.ScheduleExecution(new[] {envelope}));
        }
    }
}
