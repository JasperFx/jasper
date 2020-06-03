using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Transports.Tcp;
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
        private IListener _agent;
        private readonly AsyncRetryPolicy _policy;
        private Task _listenerTask;

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
            envelope.ReceivedAt = Address;
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
            _agent = listener;
            Address = _agent.Address;

            _listenerTask = ConsumeFromAgent();
        }

        async Task ConsumeFromAgent()
        {
            Status = ListeningStatus.Accepting;
            var callback = ((IReceiverCallback)this);
            await foreach ((Envelope Envelope, object AckObject) receivedInfo in _agent.Consume())
            {
                Envelope envelope = receivedInfo.Envelope;
                
                try
                {
                    await callback.Received(Address, new[] { envelope });

                    await _agent.Ack(receivedInfo);
                    await callback.Acknowledged(new[] { envelope });
                }
                catch (Exception e)
                {
                    _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                    await _agent.Nack(receivedInfo);
                    await callback.NotAcknowledged(new[] { envelope });
                }
            }
        }

        public Uri Address { get; set; }
        
        public ListeningStatus Status { get; set; }

        async Task<ReceivedStatus> IReceiverCallback.Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return await ProcessReceivedMessages(now, uri, messages);
        }

        Task IReceiverCallback.Acknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        Task IReceiverCallback.NotAcknowledged(Envelope[] messages)
        {
            return _persistence.DeleteIncomingEnvelopes(messages);
        }

        public Task Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // nothing
        }

        // Separated for testing here.
        public async Task<ReceivedStatus> ProcessReceivedMessages(DateTime now, Uri uri, Envelope[] envelopes)
        {
            if (_settings.Cancellation.IsCancellationRequested) return ReceivedStatus.ProcessFailure;

            try
            {
                Envelope.MarkReceived(envelopes, uri, DateTime.UtcNow, _settings.UniqueNodeId, out var scheduled, out var incoming);

                await _persistence.StoreIncoming(envelopes);

                foreach (Envelope message in incoming)
                {
                    await Enqueue(message);
                }

                _logger.IncomingBatchReceived(envelopes);

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
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
