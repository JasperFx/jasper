using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports.Sending;
using Jasper.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Local;

internal class BufferedLocalQueue : BufferedReceiver, ISendingAgent
{
    private readonly IMessageLogger _messageLogger;

    public BufferedLocalQueue(Endpoint endpoint, IJasperRuntime runtime) : base(endpoint, runtime, runtime.Pipeline)
    {
        _messageLogger = runtime.MessageLogger;
        Destination = Address = endpoint.Uri;
        Endpoint = endpoint;
    }

    public Endpoint Endpoint { get; }

    public Uri Destination { get; }
    public Uri? ReplyUri { get; set; } = TransportConstants.RepliesUri;

    public bool Latched => false;

    public bool IsDurable => Destination.IsDurable();

    public ValueTask EnqueueOutgoingAsync(Envelope envelope)
    {
        _messageLogger.Sent(envelope);
        envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri;

        if (envelope.IsScheduledForLater(DateTimeOffset.Now))
        {
            ScheduleExecution(envelope);
        }
        else
        {
            Enqueue(envelope);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask StoreAndForwardAsync(Envelope envelope)
    {
        return EnqueueOutgoingAsync(envelope);
    }

    public bool SupportsNativeScheduledSend { get; } = true;
}
