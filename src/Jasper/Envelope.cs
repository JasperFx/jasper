﻿using System;
using System.Collections.Generic;
using Jasper.Attributes;
using Jasper.Util;

namespace Jasper;

[MessageIdentity("envelope")]
public partial class Envelope
{
    public static readonly string PingMessageType = "jasper-ping";
    private byte[]? _data;

    private object? _message;
    private DateTimeOffset? _scheduledTime;
    private DateTimeOffset? _deliverBy;

    public Envelope()
    {
    }

    public Envelope(object message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Optional metadata about this message
    /// </summary>
    public Dictionary<string, string?> Headers { get; internal set; } = new();

    /// <summary>
    ///     Instruct Jasper to throw away this message if it is not successfully sent and processed
    ///     by the time specified
    /// </summary>
    public DateTimeOffset? DeliverBy
    {
        get => _deliverBy;
        set => _deliverBy = value?.ToUniversalTime();
    }

    /// <summary>
    ///     Is an acknowledgement requested
    /// </summary>
    public bool AckRequested { get; internal set; }

    /// <summary>
    ///     Used by scheduled jobs or transports with a native scheduled send functionality to have this message processed by
    ///     the receiving application at or after the designated time
    /// </summary>
    public DateTimeOffset? ScheduledTime
    {
        get => _scheduledTime;
        set => _scheduledTime = value?.ToUniversalTime();
    }

    /// <summary>
    ///     Set the DeliverBy property to have this message thrown away
    ///     if it cannot be sent before the alotted time
    /// </summary>
    /// <value></value>
    public TimeSpan DeliverWithin
    {
        set => DeliverBy = DateTimeOffset.Now.Add(value);
    }

    /// <summary>
    /// Set the ScheduleTime to now plus the value of the supplied TimeSpan
    /// </summary>
    public TimeSpan ScheduleDelay
    {
        set => ScheduledTime = DateTimeOffset.Now.Add(value);
    }

    /// <summary>
    ///     Schedule this envelope to be sent or executed
    ///     after a delay
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    public Envelope ScheduleDelayed(TimeSpan delay)
    {
        ScheduledTime = DateTimeOffset.Now.Add(delay);
        return this;
    }

    /// <summary>
    ///     Schedule this envelope to be sent or executed
    ///     at a certain time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public Envelope ScheduleAt(DateTimeOffset time)
    {
        ScheduledTime = time;
        return this;
    }

    /// <summary>
    ///     The raw, serialized message data
    /// </summary>
    public byte[]? Data
    {
        get
        {
            if (_data != null)
            {
                return _data;
            }

            if (_message == null)
            {
                throw new InvalidOperationException("Cannot ensure data is present when there is no message");
            }

            if (Serializer == null)
            {
                throw new InvalidOperationException("No data or writer is known for this envelope");
            }

            // TODO -- this is messy!
            _data = Serializer.Write(this);

            return _data;
        }
        set => _data = value;
    }

    internal int? MessagePayloadSize => _data?.Length;

    /// <summary>
    ///     The actual message to be sent or being received
    /// </summary>
    public object? Message
    {
        get => _message;
        set
        {
            MessageType = value?.GetType().ToMessageTypeName();
            _message = value;
        }
    }

    /// <summary>
    ///     Number of times that Jasper has tried to process this message. Will
    ///     reflect the current attempt number
    /// </summary>
    public int Attempts { get; internal set; }


    public DateTimeOffset SentAt { get; internal set; } = DateTimeOffset.Now;

    /// <summary>
    ///     The name of the service that sent this envelope
    /// </summary>
    public string? Source { get; internal set; }


    /// <summary>
    ///     Message type alias for the contents of this Envelope
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    ///     Location where any replies should be sent
    /// </summary>
    public Uri? ReplyUri { get; internal set; }

    /// <summary>
    ///     Mimetype of the serialized data
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     Correlating identifier for the logical workflow or system action
    /// </summary>
    public string? CorrelationId { get; internal set; }

    /// <summary>
    ///     If this message is part of a stateful saga, this property identifies
    ///     the underlying saga state object
    /// </summary>
    public string? SagaId { get; internal set; }

    /// <summary>
    ///     Id of the immediate message or workflow that caused this envelope to be sent
    /// </summary>
    public Guid ConversationId { get; internal set; }

    /// <summary>
    ///     Location that this message should be sent
    /// </summary>
    public Uri? Destination { get; set; }

    /// <summary>
    /// The open telemetry activity parent id. Jasper uses this to correctly correlate connect
    /// activity across services
    /// </summary>
    public string? ParentId { get; internal set; }

    /// <summary>
    ///     Specifies the accepted content types for the requested reply
    /// </summary>
    public string?[] AcceptedContentTypes { get; set; } = new string?[0];

    /// <summary>
    ///     Specific message id for this envelope
    /// </summary>
    public Guid Id { get; internal set; } = CombGuidIdGeneration.NewGuid();

    /// <summary>
    ///     If specified, the message type alias for the reply message that is requested for this message
    /// </summary>
    public string? ReplyRequested { get; internal set; }

    /// <summary>
    ///     Designates the topic name for outgoing messages to topic-based publish/subscribe
    ///     routing. This property is only used for routing
    /// </summary>
    public string? TopicName { get; set; }


    public override string ToString()
    {
        var text = $"Envelope #{Id}";
        if (Message != null)
        {
            text += $" ({Message.GetType().Name})";
        }

        if (Source != null)
        {
            text += $" from {Source}";
        }

        if (Destination != null)
        {
            text += $" to {Destination}";
        }


        return text;
    }


    protected bool Equals(Envelope other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is Envelope envelope)
        {
            return Equals(envelope);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    ///     Should the processing of this message be scheduled for a later time
    /// </summary>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    public bool IsScheduledForLater(DateTimeOffset utcNow)
    {
        return ScheduledTime.HasValue && ScheduledTime.Value > utcNow;
    }

    /// <summary>
    ///     Has this envelope expired according to its DeliverBy value
    /// </summary>
    /// <returns></returns>
    public bool IsExpired()
    {
        return DeliverBy.HasValue && DeliverBy <= DateTimeOffset.Now;
    }


    internal string GetMessageTypeName()
    {
        return (Message?.GetType().Name ?? MessageType)!;
    }
}
