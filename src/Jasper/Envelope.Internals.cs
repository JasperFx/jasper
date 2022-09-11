using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper;

public enum EnvelopeStatus
{
    Outgoing,
    Scheduled,
    Incoming
}

// Why is this a partial you ask?
// The elements in this file are all things that only matter
// inside the Jasper runtime so we can keep it out of the WireProtocol
public partial class Envelope
{
    private bool _enqueued;

    internal Envelope(object message, ISendingAgent agent)
    {
        Message = message;
        Sender = agent;
        Serializer = agent.Endpoint.DefaultSerializer;
        ContentType = Serializer!.ContentType;
        Destination = agent.Destination;
        ReplyUri = agent.ReplyUri;
    }

    internal Envelope(object message, IMessageSerializer writer)
    {
        Message = message;
        Serializer = writer ?? throw new ArgumentNullException(nameof(writer));
        ContentType = writer.ContentType;
    }


    public IMessageSerializer? Serializer { get; set; }

    /// <summary>
    ///     Used by IMessageContext.Invoke<T> to denote the response type
    /// </summary>
    internal Type? ResponseType { get; set; }

    /// <summary>
    ///     Also used by IMessageContext.Invoke<T> to catch the response
    /// </summary>
    internal object? Response { get; set; }

    /// <summary>
    ///     Status according to the message persistence
    /// </summary>
    internal EnvelopeStatus Status { get; set; }

    /// <summary>
    ///     Node owner of this message. 0 denotes that no node owns this message
    /// </summary>
    internal int OwnerId { get; set; }

    internal ISendingAgent? Sender { get; set; }

    internal void MarkReceived(IListener listener, DateTimeOffset now, AdvancedSettings settings)
    {
        Listener = listener;
        Destination = listener.Address;
        if (IsScheduledForLater(now))
        {
            Status = EnvelopeStatus.Scheduled;
            OwnerId = TransportConstants.AnyNode;
        }
        else
        {
            Status = EnvelopeStatus.Incoming;
            OwnerId = settings.UniqueNodeId;
        }
    }

    public IListener? Listener { get; private set; }

    /// <summary>
    ///     Create a new Envelope that is a response to the current
    ///     Envelope
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal Envelope CreateForResponse(object message)
    {
        var child = ForSend(message);
        child.CorrelationId = CorrelationId;
        child.ConversationId = Id;

        if (message.GetType().ToMessageTypeName() == ReplyRequested)
        {
            child.Destination = ReplyUri;
            child.AcceptedContentTypes = AcceptedContentTypes;
        }

        return child;
    }

    internal Envelope ForSend(object message)
    {
        return new Envelope
        {
            Message = message,
            CorrelationId = Id.ToString(),
            ConversationId = Id,
            SagaId = SagaId
        };
    }

    internal async ValueTask StoreAndForwardAsync()
    {
        if (_enqueued)
        {
            throw new InvalidOperationException("This envelope has already been enqueued");
        }

        if (Sender == null)
        {
            throw new InvalidOperationException("This envelope has not been routed");
        }

        _enqueued = true;

        await Sender.StoreAndForwardAsync(this);
    }

    internal ValueTask QuickSendAsync()
    {
        if (_enqueued)
        {
            throw new InvalidOperationException("This envelope has already been enqueued");
        }

        if (Sender == null)
        {
            throw new InvalidOperationException("This envelope has not been routed");
        }

        _enqueued = true;

        return Sender.EnqueueOutgoingAsync(this);
    }

    /// <summary>
    ///     Is this envelope for a "ping" message used by Jasper to evaluate
    ///     whether a sending endpoint can be restarted
    /// </summary>
    /// <returns></returns>
    public bool IsPing()
    {
        return MessageType == PingMessageType;
    }

    internal static Envelope ForPing(Uri destination)
    {
        return new Envelope
        {
            MessageType = PingMessageType,
            Data = new byte[] { 1, 2, 3, 4 },
            ContentType = "jasper/ping",
            Destination = destination
        };
    }

    internal void WriteTags(Activity activity)
    {
        activity.MaybeSetTag(JasperTracing.MessagingSystem, Destination?.Scheme); // This needs to vary
        activity.MaybeSetTag(JasperTracing.MessagingDestination, Destination);
        activity.SetTag(JasperTracing.MessagingMessageId, Id);
        activity.SetTag(JasperTracing.MessagingConversationId, CorrelationId);
        activity.SetTag(JasperTracing.MessageType, MessageType); // Jasper specific
        activity.MaybeSetTag(JasperTracing.PayloadSizeBytes, MessagePayloadSize);

        activity.MaybeSetTag(JasperTracing.MessagingConversationId, ConversationId);
    }
}
