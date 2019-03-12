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
    public class DurableLoopbackSendingAgent : ISendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistence _persistence;
        private readonly IWorkerQueue _queues;
        private readonly SerializationGraph _serializers;

        public DurableLoopbackSendingAgent(Uri destination, IWorkerQueue queues, IEnvelopePersistence persistence,
            SerializationGraph serializers, ITransportLogger logger)
        {
            _queues = queues;
            _serializers = serializers;
            _logger = logger;

            _persistence = persistence;

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
            envelope.Callback = new DurableCallback(envelope, _queues, _persistence, _logger);

            return _queues.Enqueue(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            writeMessageData(envelope);

            await _persistence.StoreIncoming(envelope);

            await EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) writeMessageData(envelope);

            await _persistence.StoreIncoming((Envelope[]) envelopes);

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
