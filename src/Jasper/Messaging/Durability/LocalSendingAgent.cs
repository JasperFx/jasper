using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class LocalSendingAgent : ISendingAgent
    {
        private readonly IWorkerQueue _queues;
        private readonly SerializationGraph _serializers;
        private readonly IRetries _retries;
        private readonly IEnvelopePersistor _persistor;
        public Uri Destination { get; }

        public LocalSendingAgent(Uri destination, IWorkerQueue queues, IEnvelopePersistor persistor,
            SerializationGraph serializers, IRetries retries)
        {
            _queues = queues;
            _serializers = serializers;
            _retries = retries;

            _persistor = persistor;

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
            envelope.Callback = new DurableCallback(envelope, _queues, _persistor, _retries);

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
            foreach (var envelope in envelopes)
            {
                writeMessageData(envelope);
            }

            await _persistor.StoreIncoming(envelopes);

            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
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
