using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.Marten.Persistence.Resiliency;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class LocalSendingAgent : ISendingAgent
    {
        private readonly IWorkerQueue _queues;
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _marker;
        private readonly SerializationGraph _serializers;
        private readonly MartenRetries _retries;
        private readonly CompositeTransportLogger _logger;
        public Uri Destination { get; }

        public LocalSendingAgent(Uri destination, IWorkerQueue queues, IDocumentStore store, EnvelopeTables marker, SerializationGraph serializers, MartenRetries retries, CompositeTransportLogger logger)
        {
            _queues = queues;
            _store = store;
            _marker = marker;
            _serializers = serializers;
            _retries = retries;
            _logger = logger;
            Destination = destination;
        }

        public void Dispose()
        {
            // nothing
        }

        public Uri DefaultReplyUri { get; set; }

        public bool Latched => false;

        public bool IsDurable => true;

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.Callback = new MartenCallback(envelope, _queues, _store, _marker, _retries, _logger);

            return _queues.Enqueue(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            using (var session = _store.LightweightSession())
            {
                writeMessageData(envelope);
                session.StoreIncoming(_marker, envelope);
                await session.SaveChangesAsync();
            }

            await EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            using (var session = _store.LightweightSession())
            {
                foreach (var envelope in envelopes)
                {
                    writeMessageData(envelope);

                    session.StoreIncoming(_marker, envelope);
                }



                await session.SaveChangesAsync();

                foreach (var envelope in envelopes)
                {
                    await EnqueueOutgoing(envelope);
                }
            }
        }

        private void writeMessageData(Envelope envelope)
        {
            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                var writer = _serializers.JsonWriterFor(envelope.Message.GetType());
                envelope.Data = writer.Write(envelope.Message);
                envelope.ContentType = writer.ContentType;
            }
        }

        public void Start()
        {
            // Nothing
        }
    }
}
