using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.WorkerQueues
{
    public class DurableWorkerQueue : IWorkerQueue
    {
        private readonly AdvancedSettings _settings;
        private readonly IEnvelopePersistence _persistence;
        private readonly ITransportLogger _logger;
        private readonly ActionBlock<Envelope> _receiver;
        private IListener _agent;

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

                    await pipeline.Invoke(envelope);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogException(e);
                }
            }, endpoint.ExecutionOptions);
        }

        public int QueuedCount => _receiver.InputCount;
        public Task Enqueue(Envelope envelope)
        {

            envelope.Callback = new DurableCallback(envelope, this, _persistence, _logger);
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
            _agent.Start(this);
        }

        public Uri Address => _agent.Address;

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
            return _persistence.DeleteIncomingEnvelopes(messages);
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

                await _persistence.StoreIncoming(envelopes);


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
    }
}
