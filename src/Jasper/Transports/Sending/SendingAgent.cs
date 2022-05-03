using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Sending
{
    public abstract class SendingAgent : ISendingAgent, ISenderCallback, ICircuit
    {
        private readonly ILogger _logger;
        private readonly IMessageLogger _messageLogger;
        protected readonly ISender _sender;
        protected readonly AdvancedSettings _settings;
        private int _failureCount;
        private CircuitWatcher? _circuitWatcher;

        protected readonly Func<Envelope, Task> _senderDelegate;


        public SendingAgent(ILogger logger, IMessageLogger messageLogger, ISender sender, AdvancedSettings settings, Endpoint endpoint)
        {
            _logger = logger;
            _messageLogger = messageLogger;
            _sender = sender;
            _settings = settings;
            Endpoint = endpoint;

            _senderDelegate = sendWithExplicitHandlingAsync;
            if (_sender is ISenderRequiresCallback)
            {
                _senderDelegate = sendWithCallbackHandlingAsync;
            }


            _sending = new ActionBlock<Envelope>(_senderDelegate, Endpoint.ExecutionOptions);
        }

        public Endpoint Endpoint { get; }

        public Uri? ReplyUri { get; set; }

        public Uri Destination => _sender.Destination;

        public void Dispose()
        {
            _sender.Dispose();
        }

        public bool Latched { get; private set; }
        public abstract bool IsDurable { get; }

        private void setDefaults(Envelope envelope)
        {
            envelope.Status = EnvelopeStatus.Outgoing;
            envelope.OwnerId = _settings.UniqueNodeId;
            envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri;
        }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            setDefaults(envelope);
           _sending.Post(envelope);
           _messageLogger.Sent(envelope);

           return Task.CompletedTask;
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            setDefaults(envelope);

            await storeAndForwardAsync(envelope);

            _messageLogger.Sent(envelope);
        }

        protected abstract Task storeAndForwardAsync(Envelope envelope);

        public Task<bool> TryToResume(CancellationToken cancellationToken)
        {
            return _sender.Ping(cancellationToken);
        }
        TimeSpan ICircuit.RetryInterval => Endpoint.PingIntervalForCircuitResume;

        Task ICircuit.Resume(CancellationToken cancellationToken)
        {
            _circuitWatcher = null;

            Unlatch();

            return afterRestartingAsync(_sender);
        }

        protected abstract Task afterRestartingAsync(ISender sender);

        public abstract Task Successful(Envelope outgoing);

        private ActionBlock<Envelope> _sending;
        public Task LatchAndDrainAsync()
        {
            Latched = true;

            _sending.Complete();

            _logger.CircuitBroken(Destination);

            return Task.CompletedTask;
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Latched = false;
        }

        private async Task sendWithCallbackHandlingAsync(Envelope envelope)
        {
            try
            {
                await _sender.Send(envelope);
            }
            catch (Exception e)
            {
                try
                {
                    await ProcessingFailure(envelope, e);
                }
                catch (Exception? exception)
                {
                    _logger.LogError(exception, "Error while trying to process a failure");
                }
            }
        }

        private async Task sendWithExplicitHandlingAsync(Envelope envelope)
        {
            try
            {
                await _sender.Send(envelope);

                await Successful(envelope);
            }
            catch (Exception e)
            {
                try
                {
                    await ProcessingFailure(envelope, e);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while trying to process a batch send failure");
                }
            }
        }

        private async Task markFailedAsync(OutgoingMessageBatch batch)
        {
            // If it's already latched, just enqueue again
            if (Latched)
            {
                await EnqueueForRetryAsync(batch);
                return;
            }

            _failureCount++;

            if (_failureCount >= Endpoint.FailuresBeforeCircuitBreaks)
            {
                await LatchAndDrainAsync();
                await EnqueueForRetryAsync(batch);

                _circuitWatcher = new CircuitWatcher(this, _settings.Cancellation);
            }
            else
            {
                foreach (var envelope in batch.Messages)
                {
#pragma warning disable 4014
                    _senderDelegate(envelope);
#pragma warning restore 4014
                }
            }
        }


        public abstract Task EnqueueForRetryAsync(OutgoingMessageBatch batch);


        public Task MarkSuccessAsync()
        {
            _failureCount = 0;
            Unlatch();
            _circuitWatcher = null;

            return Task.CompletedTask;
        }


        Task ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            return markFailedAsync(outgoing);
        }

        Task ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            // Can't really happen now, but what the heck.
            var exception = new Exception("Serialization failure with outgoing envelopes " +
                                          outgoing.Messages.Select(x => x.ToString()).Join(", "));
            _logger.LogError(exception, "Serialization failure");

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
            return markFailedAsync(outgoing);
        }

        public Task ProcessingFailure(Envelope outgoing, Exception? exception)
        {
            var batch = new OutgoingMessageBatch(outgoing.Destination, new[] { outgoing });
            _logger.OutgoingBatchFailed(batch, exception);
            return markFailedAsync(batch);
        }

        public Task ProcessingFailure(OutgoingMessageBatch outgoing, Exception? exception)
        {
            _logger.LogError(exception,
                message: "Failure trying to send a message batch to {Destination}", outgoing.Destination);
            _logger.OutgoingBatchFailed(outgoing, exception);
            return markFailedAsync(outgoing);
        }

        Task ISenderCallback.SenderIsLatched(OutgoingMessageBatch outgoing)
        {
            return markFailedAsync(outgoing);
        }

        public abstract Task Successful(OutgoingMessageBatch outgoing);

        public bool SupportsNativeScheduledSend => _sender.SupportsNativeScheduledSend;

    }
}
