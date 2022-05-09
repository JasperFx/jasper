using System;
using System.Threading.Tasks;
using Jasper.Configuration;

namespace Jasper.Transports.Sending;

public interface ISendingAgent
{
    Uri Destination { get; }
    Uri? ReplyUri { get; set; }
    bool Latched { get; }


    bool IsDurable { get; }

    bool SupportsNativeScheduledSend { get; }

    Endpoint Endpoint { get; }

    Task EnqueueOutgoingAsync(Envelope envelope);

    Task StoreAndForwardAsync(Envelope envelope);
}
