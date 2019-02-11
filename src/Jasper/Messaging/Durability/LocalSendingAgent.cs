using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class LocalSendingAgent : ISendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistor _persistor;
        private readonly IWorkerQueue _queues;
        private readonly IRetries _retries;
        private readonly SerializationGraph _serializers;

        public LocalSendingAgent(Uri destination, IWorkerQueue queues, IEnvelopePersistor persistor,
            SerializationGraph serializers, IRetries retries, ITransportLogger logger)
        {
            _queues = queues;
            _serializers = serializers;
            _retries = retries;
            _logger = logger;

            _persistor = persistor;

            Destination = destination;
        }

        public Uri Destination { get; }

        public void Dispose()
        {
            // nothing
        }

        public Uri DefaultReplyUri { get; set; }

        public bool Latched => false;

        public bool IsDurable => true;

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.Callback = new DurableCallback(envelope, _queues, _persistor, _retries, _logger);

            return _queues.Enqueue(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            writeMessageData(envelope);

            await _persistor.StoreIncoming(envelope);

            await EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) writeMessageData(envelope);

            await _persistor.StoreIncoming((Envelope[]) envelopes);

            foreach (var envelope in envelopes) await EnqueueOutgoing(envelope);
        }

        public void Start()
        {
            // Nothing
        }

        public bool SupportsNativeScheduledSend { get; } = true;

        public int QueuedCount { get; } = 0;

        private void writeMessageData(Envelope envelope)
        {
            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                var writer = _serializers.JsonWriterFor(envelope.Message.GetType());
                envelope.Data = writer.Write(envelope.Message);
                envelope.ContentType = writer.ContentType;
            }
        }
    }
}
