using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Bus.Transports.Sending
{
    public abstract class SendingAgent : ISendingAgent, ISenderCallback
    {
        protected readonly ISender _sender;
        private readonly CompositeTransportLogger _logger;
        protected readonly RetryAgent _retries;

        protected SendingAgent(Uri destination, ISender sender, CompositeTransportLogger logger, BusSettings settings, RetryAgent retries)
        {
            _sender = sender;
            _logger = logger;
            Destination = destination;

            _retries = retries;
        }

        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }
        public bool Latched => _sender.Latched;

        public abstract bool IsDurable { get; }

        public abstract Task EnqueueOutgoing(Envelope envelope);
        public abstract Task StoreAndForward(Envelope envelope);
        public abstract Task StoreAndForwardMany(IEnumerable<Envelope> envelopes);

        public void Start()
        {
            _sender.Start(this);
        }

        public abstract Task Successful(OutgoingMessageBatch outgoing);

        public Task TimedOut(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            return _retries.MarkFailed(outgoing);
        }

        public Task SerializationFailure(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            // Can't really happen now, but what the heck.
            _logger.LogException(new Exception("Serialization failure with outgoing envelopes " + outgoing.Messages.Select(x => x.ToString()).Join(", ")));

            return Task.CompletedTask;
        }

        public Task QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing, new QueueDoesNotExistException(outgoing));

            return Task.CompletedTask;
        }

        public Task ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            _logger.OutgoingBatchFailed(outgoing);
            return _retries.MarkFailed(outgoing);
        }

        public Task ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            _logger.LogException(exception, message:$"Failure trying to send a message batch to {outgoing.Destination}");
            _logger.OutgoingBatchFailed(outgoing, exception);
            return _retries.MarkFailed(outgoing);
        }

        public Task SenderIsLatched(OutgoingMessageBatch outgoing)
        {
            return _retries.MarkFailed(outgoing);
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }

    public class QueueDoesNotExistException : Exception
    {
        public QueueDoesNotExistException(OutgoingMessageBatch outgoing) : base($"Queue '{outgoing.Destination.QueueName()}' does not exist at {outgoing.Destination}")
        {

        }
    }
}
