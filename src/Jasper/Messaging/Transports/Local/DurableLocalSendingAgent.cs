using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Transports.Local
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


            ReplyUri = TransportConstants.RepliesUri;
        }

        public Uri Destination { get; }

        public void Dispose()
        {
            // nothing
        }

        public Uri ReplyUri { get; set; }

        public bool Latched => false;

        public bool IsDurable => true;

        public Task EnqueueOutgoing(Envelope envelope)
        {
            _messageLogger.Sent(envelope);
            envelope.Callback = new DurableCallback(envelope, this, _persistence, _logger);

            return Enqueue(envelope);
        }

        public async Task StoreAndForward(Envelope envelope)
        {
            _messageLogger.Sent(envelope);
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
