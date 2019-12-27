using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Runtime.Invocation;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports.Local
{
    public class LightweightLocalSendingAgent : LightweightWorkerQueue, ISendingAgent
    {
        private readonly IMessageLogger _messageLogger;

        public LightweightLocalSendingAgent(Endpoint endpoint, ITransportLogger logger,
            IHandlerPipeline pipeline, AdvancedSettings settings, IMessageLogger messageLogger) : base(endpoint, logger, pipeline, settings)
        {
            _messageLogger = messageLogger;
            Destination = endpoint.Uri;
        }

        public Uri Destination { get; }
        public Uri ReplyUri { get; set; } = TransportConstants.RepliesUri;

        public void Dispose()
        {
            // Nothing
        }

        public bool Latched { get; } = false;

        public bool IsDurable => Destination.IsDurable();

        public Task EnqueueOutgoing(Envelope envelope)
        {
            _messageLogger.Sent(envelope);
            envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri;
            envelope.ReceivedAt = Destination;
            envelope.Callback = new LightweightCallback(this);

            return envelope.IsDelayed(DateTime.UtcNow)
                ? ScheduleExecution(envelope)
                : Enqueue(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

    }
}
