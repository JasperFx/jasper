using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    public class LightweightSendingAgent : SendingAgent
    {
        public LightweightSendingAgent(Uri destination, ISender sender, CompositeTransportLogger logger, BusSettings settings)
        : base(destination, sender, logger, settings, new LightweightRetryAgent(sender, settings.Retries))
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
            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public override Task Successful(OutgoingMessageBatch outgoing)
        {
            return _retries.MarkSuccess();
        }
    }
}
