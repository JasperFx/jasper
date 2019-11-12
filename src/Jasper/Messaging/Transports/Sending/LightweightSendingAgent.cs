using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public class LightweightSendingAgent : SendingAgent
    {
        public LightweightSendingAgent(Uri destination, ISender sender, ITransportLogger logger,
            AdvancedSettings settings)
            : base(destination, sender, logger, new LightweightRetryAgent(sender, settings))
        {
        }

        public override bool IsDurable => false;

        public override Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
            return _sender.Enqueue(envelope);
        }

        public override Task StoreAndForward(Envelope envelope)
        {
            // Same thing here
            return EnqueueOutgoing(envelope);
        }

        public override Task Successful(OutgoingMessageBatch outgoing)
        {
            return _retries.MarkSuccess();
        }

        public override Task Successful(Envelope outgoing)
        {
            return _retries.MarkSuccess();
        }
    }
}
