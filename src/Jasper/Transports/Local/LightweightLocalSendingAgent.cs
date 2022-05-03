using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports.Sending;
using Jasper.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Local
{
    public class LightweightLocalSendingAgent : LightweightWorkerQueue, ISendingAgent
    {
        private readonly IMessageLogger _messageLogger;

        public LightweightLocalSendingAgent(Endpoint endpoint, ILogger logger,
            IHandlerPipeline pipeline, AdvancedSettings? settings, IMessageLogger messageLogger) : base(endpoint, logger, pipeline, settings)
        {
            _messageLogger = messageLogger;
            Destination = Address = endpoint.Uri;
            Endpoint = endpoint;
        }

        public Endpoint Endpoint { get; }

        public Uri Destination { get; }
        public Uri? ReplyUri { get; set; } = TransportConstants.RepliesUri;

        public bool Latched { get; } = false;

        public bool IsDurable => Destination.IsDurable();

        public Task EnqueueOutgoing(Envelope envelope)
        {
            _messageLogger.Sent(envelope);
            envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri;

            return envelope.IsScheduledForLater(DateTime.UtcNow)
                ? ScheduleExecutionAsync(envelope)
                : EnqueueAsync(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

    }
}
