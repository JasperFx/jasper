using System;
using System.Threading.Tasks;
using Jasper.Configuration;

namespace Jasper.Transports.Sending
{
    public interface ISendingAgent : IDisposable
    {
        Uri Destination { get; }
        Uri? ReplyUri { get; set; }
        bool Latched { get; }


        bool IsDurable { get; }

        bool SupportsNativeScheduledSend { get; }

        // This would be called in the future by the outbox, assuming
        // that the envelope is already persisted and just needs to be sent out
        Task EnqueueOutgoing(Envelope envelope);

        // This would be called by the EnvelopeSender if invoked
        // indirectly
        Task StoreAndForward(Envelope envelope);

        Endpoint Endpoint { get; }

    }
}
