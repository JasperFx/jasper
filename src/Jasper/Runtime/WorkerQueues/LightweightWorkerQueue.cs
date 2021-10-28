using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues
{
    public class LightweightWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling
    {
        private readonly ITransportLogger _logger;
        private readonly AdvancedSettings _settings;
        private readonly ActionBlock<Envelope> _receiver;
        private readonly InMemoryScheduledJobProcessor _scheduler;
        private IListener _listener;

        public LightweightWorkerQueue(Endpoint endpoint, ITransportLogger logger,
            IHandlerPipeline pipeline, AdvancedSettings settings)
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
                        envelope.ContentType = "application/json";
                    }

                    await Pipeline.Invoke(envelope, this);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogException(e);
                }
            }, endpoint.ExecutionOptions);
        }

        public IHandlerPipeline Pipeline { get; }

        public int QueuedCount => _receiver.InputCount;
        public Task Enqueue(Envelope envelope)
        {
            if (envelope.IsPing()) return Task.CompletedTask;

            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public Task ScheduleExecution(Envelope envelope)
        {
            if (!envelope.ExecutionTime.HasValue) throw new ArgumentOutOfRangeException(nameof(envelope), $"There is no {nameof(Envelope.ExecutionTime)} value");

            _scheduler.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }


        public void StartListening(IListener listener)
        {
            _listener = listener;
            _listener.Start(this);

            Address = _listener.Address;
        }

        public Uri Address { get; set; }


        public ListeningStatus Status
        {
            get => _listener.Status;
            set => _listener.Status = value;
        }

        Task IListeningWorkerQueue.Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return ProcessReceivedMessages(now, uri, messages);
        }

        public async Task Received(Uri uri, Envelope envelope)
        {
            var now = DateTime.UtcNow;
            envelope.MarkReceived(uri, now, _settings.UniqueNodeId);

            if (envelope.IsExpired()) return;

            if (envelope.Status == EnvelopeStatus.Scheduled)
            {
                _scheduler.Enqueue(envelope.ExecutionTime.Value, envelope);
            }
            else
            {
                await Enqueue(envelope);
            }

            await _listener.Complete(envelope);

            _logger.IncomingReceived(envelope);
        }

        public void Dispose()
        {
            _receiver.Complete();
        }

        // Separated for testing here.
        public async Task ProcessReceivedMessages(DateTime now, Uri uri, Envelope[] envelopes)
        {
            if (_settings.Cancellation.IsCancellationRequested) throw new OperationCanceledException();

            foreach (var envelope in envelopes)
            {
                envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);
                await Enqueue(envelope);
                await _listener.Complete(envelope);
            }

            _logger.IncomingBatchReceived(envelopes);

        }

        Task IChannelCallback.Complete(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        async Task IChannelCallback.Defer(Envelope envelope)
        {
            if (_listener == null)
            {
                await Enqueue(envelope);
                return;
            }

            var nativelyRequeued = await _listener.TryRequeue(envelope);
            if (!nativelyRequeued)
            {
                await Enqueue(envelope);
            }
        }

        Task IHasNativeScheduling.MoveToScheduledUntil(Envelope envelope, DateTimeOffset time)
        {
            envelope.ExecutionTime = time;
            return ScheduleExecution(envelope);
        }
    }
}
