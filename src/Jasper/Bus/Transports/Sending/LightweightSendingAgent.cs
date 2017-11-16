using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public class LightweightSendingAgent : ISendingAgent, ISenderCallback
    {
        private readonly ISender _sender;
        private readonly CompositeLogger _logger;
        private readonly LightweightRetryAgent _retries;

        public LightweightSendingAgent(Uri destination, ISender sender, CompositeLogger logger, BusSettings settings)
        {
            _sender = sender;
            _logger = logger;
            Destination = destination;

            _retries = new LightweightRetryAgent(_sender, settings.LightweightRetry);
        }

        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
            return _sender.Enqueue(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            // Same thing here
            return EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public void Start()
        {
            _sender.Start(this);
        }

        public void Successful(OutgoingMessageBatch outgoing)
        {
            _retries.MarkSuccess();
        }

        public void TimedOut(OutgoingMessageBatch outgoing)
        {
            _retries.MarkFailed(outgoing);
        }

        public void SerializationFailure(OutgoingMessageBatch outgoing)
        {
            // Can't really happen now, but what the heck.
            _logger.LogException(new Exception("Serialization failure with outgoing envelopes " + outgoing.Messages.Select(x => x.ToString()).Join(", ")));
        }

        public void QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            // Doesn't really happen in Jasper
        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            _retries.MarkFailed(outgoing);
        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            _logger.LogException(exception, $"Failure trying to send a message batch to {outgoing.Destination}");
            _retries.MarkFailed(outgoing);

        }

        public void SenderIsLatched(OutgoingMessageBatch outgoing)
        {
            _retries.MarkFailed(outgoing);
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }
}
