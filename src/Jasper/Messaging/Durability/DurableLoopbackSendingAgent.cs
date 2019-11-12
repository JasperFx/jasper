using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class DurableLoopbackSendingAgent : ISendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly AdvancedSettings _settings;
        private readonly IEnvelopePersistence _persistence;
        private readonly IWorkerQueue _queues;
        private readonly MessagingSerializationGraph _serializers;

        public DurableLoopbackSendingAgent(Uri destination, IWorkerQueue queues, IEnvelopePersistence persistence,
            MessagingSerializationGraph serializers, ITransportLogger logger, AdvancedSettings settings)
        {
            _queues = queues;
            _serializers = serializers;
            _logger = logger;
            _settings = settings;

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

            // TODO -- have to watch this one
            envelope.Status = envelope.IsDelayed(DateTime.UtcNow)
                ? TransportConstants.Scheduled
                : TransportConstants.Incoming;

            envelope.OwnerId = envelope.Status == TransportConstants.Incoming
                ? _settings.UniqueNodeId
                : TransportConstants.AnyNode;

            await _persistence.StoreIncoming(envelope);

            if (envelope.Status == TransportConstants.Incoming)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public void Start()
        {
            // Nothing
        }

        public bool SupportsNativeScheduledSend { get; } = true;


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
