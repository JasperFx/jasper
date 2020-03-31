using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Jasper.Transports.Tcp;

namespace Jasper.Runtime.WorkerQueues
{
    public class LightweightWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling
    {
        private readonly ITransportLogger _logger;
        private readonly AdvancedSettings _settings;
        private readonly ActionBlock<Envelope> _receiver;
        private readonly InMemoryScheduledJobProcessor _scheduler;
        private IListener _agent;

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
            _agent = listener;
            _agent.Start(this);

            Address = _agent.Address;
        }

        public Uri Address { get; set; }


        public ListeningStatus Status
        {
            get => _agent.Status;
            set => _agent.Status = value;
        }

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
            return Task.CompletedTask;
        }

        Task IReceiverCallback.Failed(Exception exception, Envelope[] messages)
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


                foreach (var envelope in scheduled)
                {
                    _scheduler.Enqueue(envelope.ExecutionTime.Value, envelope);
                }


                foreach (var message in incoming)
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

        Task IChannelCallback.Complete(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        Task IChannelCallback.Defer(Envelope envelope)
        {
            return Enqueue(envelope);
        }

        Task IHasNativeScheduling.MoveToScheduledUntil(Envelope envelope, DateTimeOffset time)
        {
            envelope.ExecutionTime = time;
            return ScheduleExecution(envelope);
        }
    }
}
