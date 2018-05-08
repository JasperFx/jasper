using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Sending
{
    public interface ISendingAgent : IDisposable
    {
        Uri Destination { get; }
        Uri DefaultReplyUri { get; set; }
        bool Latched { get; }


        bool IsDurable { get; }

        // This would be called in the future by the outbox, assuming
        // that the envelope is already persisted and just needs to be sent out
        Task EnqueueOutgoing(Envelope envelope);

        // This would be called by the EnvelopeSender if invoked
        // indirectly
        Task StoreAndForward(Envelope envelope);

        Task StoreAndForwardMany(IEnumerable<Envelope> envelopes);

        void Start();

        int QueuedCount { get; }
    }
}
