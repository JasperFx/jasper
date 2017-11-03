using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public class LightweightSendingAgent : ISendingAgent, ISenderCallback
    {
        private readonly ISender _sender;

        public LightweightSendingAgent(Uri destination, ISender sender)
        {
            _sender = sender;
            Destination = destination;
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

        public void Start()
        {
            _sender.Start(this);
        }

        public void Successful(OutgoingMessageBatch outgoing)
        {
            // Nothing
        }

        public void TimedOut(OutgoingMessageBatch outgoing)
        {
            // Retry a time or two?
        }

        public void SerializationFailure(OutgoingMessageBatch outgoing)
        {
            // log?
        }

        public void QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            // Doesn't really happen in Jasper
        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            // log?
        }

        public void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            // log, maybe retry?
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }
    }
}
