using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using Marten;

namespace Jasper.Marten
{
    public class MartenBackedSendingAgent : ISendingAgent, ISenderCallback
    {
        public Uri Destination { get; }
        private readonly IDocumentStore _store;
        private readonly ISender _sender;
        private readonly CancellationToken _cancellation;
        private readonly CompositeLogger _logger;

        public MartenBackedSendingAgent(Uri destination, IDocumentStore store, ISender sender, CancellationToken cancellation, CompositeLogger logger)
        {
            Destination = destination;
            _store = store;
            _sender = sender;
            _cancellation = cancellation;
            _logger = logger;
        }

        public Uri DefaultReplyUri { get; set; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.EnsureData();

            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;

            return _sender.Enqueue(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            envelope.EnsureData();

            using (var session = _store.LightweightSession())
            {
                session.Store(envelope);
                await session.SaveChangesAsync(_cancellation);
            }

            await EnqueueOutgoing(envelope);
        }

        public void Start()
        {
            _sender.Start(this);
        }

        public void Successful(OutgoingMessageBatch outgoing)
        {
            using (var session = _store.LightweightSession())
            {
                session.Delete(outgoing.Messages);
                session.SaveChanges();
            }
        }

        public void Dispose()
        {
            _sender?.Dispose();
        }

        void ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        private void processRetry(OutgoingMessageBatch outgoing)
        {
            // TODO -- so, um, do something here.
        }

        void ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            _logger.LogException(exception, message:$"Failed while trying to send messages with {nameof(MartenBackedSendingAgent)}");

            processRetry(outgoing);
        }
    }
}
