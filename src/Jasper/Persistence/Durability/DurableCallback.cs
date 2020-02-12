using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Logging;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Polly;
using Polly.Retry;

namespace Jasper.Persistence.Durability
{
    public class DurableCallback : IMessageCallback, IHasDeadLetterQueue, IHasNativeScheduling
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

        public Task Complete()
        {
            return _policy.ExecuteAsync(() => _persistence.DeleteIncomingEnvelope(_envelope));
        }

        public Task MoveToErrors(Exception exception)
        {
            var errorReport = new ErrorReport(_envelope, exception);

            return _policy.ExecuteAsync(() => _persistence.MoveToDeadLetterStorage(new[] {errorReport}));
        }

        public async Task Defer()
        {
            _envelope.Attempts++;

            await _queue.Enqueue(_envelope);

            await _policy.ExecuteAsync(() => _persistence.IncrementIncomingEnvelopeAttempts(_envelope));
        }

        public Task MoveToScheduledUntil(DateTimeOffset time)
        {
            _envelope.OwnerId = TransportConstants.AnyNode;
            _envelope.ExecutionTime = time;
            _envelope.Status = EnvelopeStatus.Scheduled;

            return _policy.ExecuteAsync(() => _persistence.ScheduleExecution(new[] {_envelope}));
        }
    }
}
