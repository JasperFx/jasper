using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public abstract class SendingAgent : ISendingAgent, ISenderCallback
    {
        private readonly ITransportLogger _logger;
        private readonly IMessageLogger _messageLogger;
        protected readonly ISender _sender;
        protected readonly AdvancedSettings _settings;
        private int _failureCount;
        private Pinger _pinger;

        public SendingAgent(ITransportLogger logger, IMessageLogger messageLogger, ISender sender,
            AdvancedSettings settings)
        {
            _logger = logger;
            _messageLogger = messageLogger;
            _sender = sender;
            _settings = settings;
        }

        public Uri ReplyUri { get; set; }

        public Uri Destination => _sender.Destination;

        public void Dispose()
        {
            _pinger?.Dispose();
            _sender.Dispose();
        }

        public bool Latched => _sender.Latched;
        public abstract bool IsDurable { get; }

        private void setDefaults(Envelope envelope)
        {
            envelope.Status = TransportConstants.Outgoing;
            envelope.EnsureData();
            envelope.OwnerId = _settings.UniqueNodeId;
            envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri;
        }

        public async Task EnqueueOutgoing(Envelope envelope)
        {
            setDefaults(envelope);
            await _sender.Enqueue(envelope);
            _messageLogger.Sent(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            setDefaults(envelope);

            await storeAndForward(envelope);

            _messageLogger.Sent(envelope);
        }

        protected abstract Task storeAndForward(Envelope envelope);

        public bool SupportsNativeScheduledSend => _sender.SupportsNativeScheduledSend;


        public async Task MarkFailed(OutgoingMessageBatch batch)
        {
            // If it's already latched, just enqueue again
            if (_sender.Latched)
            {
                await EnqueueForRetry(batch);
                return;
            }

            _failureCount++;

            if (_failureCount >= _settings.FailuresBeforeCircuitBreaks)
            {
                await _sender.LatchAndDrain();
                await EnqueueForRetry(batch);
                _pinger = new Pinger(_sender, _settings.Cooldown, restartSending);
            }
            else
            {
                foreach (var envelope in batch.Messages)
                {
#pragma warning disable 4014
                    _sender.Enqueue(envelope);
#pragma warning restore 4014
                }
            }
        }

        public abstract Task EnqueueForRetry(OutgoingMessageBatch batch);

        private Task restartSending()
        {
            _pinger.Dispose();
            _pinger = null;

            _sender.Unlatch();

            return afterRestarting(_sender);
        }

        protected abstract Task afterRestarting(ISender sender);

        public Task MarkSuccess()
        {
            _failureCount = 0;
            _sender.Unlatch();
            _pinger?.Dispose();
            _pinger = null;

            return Task.CompletedTask;
        }

        Task ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            return MarkFailed(outgoing);
        }

        Task ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            // Can't really happen now, but what the heck.
            _logger.LogException(new Exception("Serialization failure with outgoing envelopes " +
                                               outgoing.Messages.Select(x => x.ToString()).Join(", ")));

            return Task.CompletedTask;
        }

        Task ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing, new QueueDoesNotExistException(outgoing));

            return Task.CompletedTask;
        }

        Task ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            return MarkFailed(outgoing);
        }

        Task ISenderCallback.ProcessingFailure(Envelope outgoing, Exception exception)
        {
            var batch = new OutgoingMessageBatch(outgoing.Destination, new[] {outgoing});
            _logger.OutgoingBatchFailed(batch, exception);
            return MarkFailed(batch);
        }

        Task ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            _logger.LogException(exception,
                message: $"Failure trying to send a message batch to {outgoing.Destination}");
            _logger.OutgoingBatchFailed(outgoing, exception);
            return MarkFailed(outgoing);
        }

        Task ISenderCallback.SenderIsLatched(OutgoingMessageBatch outgoing)
        {
            return MarkFailed(outgoing);
        }

        public abstract Task Successful(OutgoingMessageBatch outgoing);

        public abstract Task Successful(Envelope outgoing);


    }
}
