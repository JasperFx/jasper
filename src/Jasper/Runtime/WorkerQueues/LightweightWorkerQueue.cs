using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime.WorkerQueues
{
    public class LightweightWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling
    {
        private readonly ILogger _logger;
        private readonly AdvancedSettings? _settings;
        private readonly ActionBlock<Envelope> _receiver;
        private readonly InMemoryScheduledJobProcessor _scheduler;
        private IListener _listener;

        public LightweightWorkerQueue(Endpoint endpoint, ILogger logger,
            IHandlerPipeline pipeline, AdvancedSettings? settings)
        {
            _logger = logger;
            _settings = settings;
            Pipeline = pipeline;

            _scheduler = new InMemoryScheduledJobProcessor(this);

            endpoint.ExecutionOptions.CancellationToken = settings.Cancellation;

            _receiver = new ActionBlock<Envelope>(async envelope =>
            {
                try
                {
                    if (envelope.ContentType.IsEmpty())
                    {
                        envelope.ContentType = EnvelopeConstants.JsonContentType;
                    }

                    await Pipeline.Invoke(envelope, this);
                }
                catch (Exception? e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogError(e, "Unexpected error in Pipeline invocation");
                }
            }, endpoint.ExecutionOptions);
        }

        public IHandlerPipeline Pipeline { get; }

        public int QueuedCount => _receiver.InputCount;
        public Task EnqueueAsync(Envelope envelope)
        {
            if (envelope.IsPing()) return Task.CompletedTask;

            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public Task ScheduleExecutionAsync(Envelope envelope)
        {
            if (!envelope.ScheduledTime.HasValue) throw new ArgumentOutOfRangeException(nameof(envelope), $"There is no {nameof(Envelope.ScheduledTime)} value");

            _scheduler.Enqueue(envelope.ScheduledTime.Value, envelope);
            return Task.CompletedTask;
        }


        public void StartListening(IListener listener)
        {
            _listener = listener;
            _listener.Start(this, _settings.Cancellation);

            Address = _listener.Address;
        }

        public Uri? Address { get; set; }


        public ListeningStatus Status
        {
            get => _listener.Status;
            set => _listener.Status = value;
        }

        Task IListeningWorkerQueue.Received(Uri? uri, Envelope?[] messages)
        {
            var now = DateTime.UtcNow;

            return ProcessReceivedMessages(now, uri, messages);
        }

        public async Task Received(Uri? uri, Envelope? envelope)
        {
            var now = DateTime.UtcNow;
            envelope.MarkReceived(uri, now, _settings.UniqueNodeId);

            if (envelope.IsExpired()) return;

            if (envelope.Status == EnvelopeStatus.Scheduled)
            {
                _scheduler.Enqueue(envelope.ScheduledTime.Value, envelope);
            }
            else
            {
                await EnqueueAsync(envelope);
            }

            await _listener.CompleteAsync(envelope);

            _logger.IncomingReceived(envelope);
        }

        public void Dispose()
        {
            _receiver.Complete();
        }

        // Separated for testing here.
        public async Task ProcessReceivedMessages(DateTime now, Uri? uri, Envelope?[] envelopes)
        {
            if (_settings.Cancellation.IsCancellationRequested) throw new OperationCanceledException();

            foreach (var envelope in envelopes)
            {
                envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);
                await EnqueueAsync(envelope);
                await _listener.CompleteAsync(envelope);
            }

            _logger.IncomingBatchReceived(envelopes);

        }

        ValueTask IChannelCallback.CompleteAsync(Envelope envelope)
        {
            return ValueTask.CompletedTask;
        }

        async ValueTask IChannelCallback.DeferAsync(Envelope envelope)
        {
            if (_listener == null)
            {
                await EnqueueAsync(envelope);
                return;
            }

            var nativelyRequeued = await _listener.TryRequeue(envelope);
            if (!nativelyRequeued)
            {
                await EnqueueAsync(envelope);
            }
        }

        Task IHasNativeScheduling.MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
        {
            envelope.ScheduledTime = time;
            return ScheduleExecutionAsync(envelope);
        }
    }
}
