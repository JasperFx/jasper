using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Polly;
using Polly.Retry;

namespace Jasper.Runtime.WorkerQueues
{
    public class DurableWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling, IHasDeadLetterQueue
    {
        private readonly AdvancedSettings _settings;
        private readonly IEnvelopePersistence _persistence;
        private readonly ITransportLogger _logger;
        private readonly ActionBlock<Envelope> _receiver;
        private IListener _listener;
        private readonly AsyncRetryPolicy _policy;

        public DurableWorkerQueue(Endpoint endpoint, IHandlerPipeline pipeline,
            AdvancedSettings settings, IEnvelopePersistence persistence, ITransportLogger logger)
        {
            _settings = settings;
            _persistence = persistence;
            _logger = logger;

            endpoint.ExecutionOptions.CancellationToken = settings.Cancellation;

            _receiver = new ActionBlock<Envelope>(async envelope =>
            {
                try
                {
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    await pipeline.Invoke(envelope, this);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogException(e);
                }
            }, endpoint.ExecutionOptions);

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => (i*100).Milliseconds()
                    , (e, timeSpan) => {
                        _logger.LogException(e);
                    });
        }

        public int QueuedCount => _receiver.InputCount;
        public Task Enqueue(Envelope envelope)
        {
            envelope.ReplyUri = envelope.ReplyUri ?? Address;
            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public Task ScheduleExecution(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public void StartListening(IListener listener)
        {
            _listener = listener;
            _listener.Start(this, _settings.Cancellation);

            Address = _listener.Address;
        }

        public Uri Address { get; set; }


        public Task Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return ProcessReceivedMessages(now, uri, messages);
        }

        public async Task Received(Uri uri, Envelope envelope)
        {
            using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryReceiveSpanName, envelope,
                ActivityKind.Consumer);
            var now = DateTime.UtcNow;
            envelope.MarkReceived(uri, now, _settings.UniqueNodeId);

            await _persistence.StoreIncoming(envelope);

            if (envelope.Status == EnvelopeStatus.Incoming)
            {
                await Enqueue(envelope);
            }

            await _listener.Complete(envelope);

            _logger.IncomingReceived(envelope);
        }


        public void Dispose()
        {
            // Might need to drain the block
            _receiver.Complete();
        }

        // Separated for testing here.
        public async Task ProcessReceivedMessages(DateTime now, Uri uri, Envelope[] envelopes)
        {
            if (_settings.Cancellation.IsCancellationRequested) throw new OperationCanceledException();

            foreach (var envelope in envelopes)
            {
                envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);
            }

            await _persistence.StoreIncoming(envelopes);

            foreach (var message in envelopes)
            {
                await Enqueue(message);
                await _listener.Complete(message);
            }

            _logger.IncomingBatchReceived(envelopes);
        }

        public Task Complete(Envelope envelope)
        {
            return _policy.ExecuteAsync(() => _persistence.DeleteIncomingEnvelope(envelope));
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            var errorReport = new ErrorReport(envelope, exception);

            return _policy.ExecuteAsync(() => _persistence.MoveToDeadLetterStorage(new[] {errorReport}));
        }

        public async Task Defer(Envelope envelope)
        {
            envelope.Attempts++;

            await Enqueue(envelope);

            await _policy.ExecuteAsync(() => _persistence.IncrementIncomingEnvelopeAttempts(envelope));
        }

        public Task MoveToScheduledUntil(Envelope envelope, DateTimeOffset time)
        {
            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.ExecutionTime = time;
            envelope.Status = EnvelopeStatus.Scheduled;

            return _policy.ExecuteAsync(() => _persistence.ScheduleExecution(new[] {envelope}));
        }
    }
}
