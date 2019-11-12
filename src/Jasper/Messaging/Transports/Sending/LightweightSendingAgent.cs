using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public class LightweightSendingAgent : SendingAgent
    {
        public LightweightSendingAgent(Uri destination, ISender sender, ITransportLogger logger,
            JasperOptions options)
            : base(destination, sender, logger, options, new LightweightRetryAgent(sender, options.Advanced))
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

        public override async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) await EnqueueOutgoing(envelope);
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
