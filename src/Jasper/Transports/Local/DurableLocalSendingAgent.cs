using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Serialization;
using Jasper.Transports.Sending;

namespace Jasper.Transports.Local
{
    public class DurableLocalSendingAgent : DurableWorkerQueue, ISendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly AdvancedSettings _settings;
        private readonly IEnvelopePersistence _persistence;
        private readonly MessagingSerializationGraph _serializers;
        private readonly IMessageLogger _messageLogger;

        public DurableLocalSendingAgent(Endpoint endpoint, IHandlerPipeline pipeline,
            AdvancedSettings settings, IEnvelopePersistence persistence, ITransportLogger logger,
            MessagingSerializationGraph serializers, IMessageLogger messageLogger) : base(endpoint, pipeline, settings, persistence, logger)
        {
            _settings = settings;
            _persistence = persistence;
            _logger = logger;
            _serializers = serializers;
            _messageLogger = messageLogger;
            Destination = endpoint.Uri;

            Endpoint = endpoint;
            ReplyUri = TransportConstants.RepliesUri;

            Address = Destination;
        }

        public Uri Destination { get; }

        public Endpoint Endpoint { get; }

        public Uri ReplyUri { get; set; }

        public bool Latched => false;

        public bool IsDurable => true;

        public Task EnqueueOutgoing(Envelope envelope)
        {
            _messageLogger.Sent(envelope);

            return Enqueue(envelope);
        }

        public async Task Forward(Envelope envelope)
        {
            _messageLogger.Sent(envelope);
            writeMessageData(envelope);

            // TODO -- have to watch this one
            envelope.Status = envelope.IsDelayed(DateTime.UtcNow)
                ? EnvelopeStatus.Scheduled
                : EnvelopeStatus.Incoming;

            envelope.OwnerId = envelope.Status == EnvelopeStatus.Incoming
                ? _settings.UniqueNodeId
                : TransportConstants.AnyNode;

            await _persistence.StoreIncoming(envelope);

            if (envelope.Status == EnvelopeStatus.Incoming)
            {
                await Enqueue(envelope);
            }
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
