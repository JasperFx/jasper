﻿namespace Jasper.Persistence.Database;

public static class DatabaseConstants
{
    public const string Id = "id";
    public const string OwnerId = "owner_id";
    public const string Destination = "destination";
    public const string DeliverBy = "deliver_by";
    public const string Body = "body";
    public const string Status = "status";

    public const string ExecutionTime = "execution_time";
    public const string Attempts = "attempts";
    public const string Source = "source";
    public const string MessageType = "message_type";

    public const string Explanation = "explanation";
    public const string ExceptionText = "exception_text";
    public const string ExceptionType = "exception_type";
    public const string ExceptionMessage = "exception_message";

    public const string OutgoingTable = "jasper_outgoing_envelopes";
    public const string IncomingTable = "jasper_incoming_envelopes";
    public const string DeadLetterTable = "jasper_dead_letters";

    public const string CorrelationId = "correlation_id"; // add to all
    public const string CausationId = "causation_id"; // add to all
    public const string SagaId = "saga_id"; // add to all
    public const string ParentId = "parent_id"; // add to all
    public const string ContentType = "content_type"; // add to all
    public const string ReplyRequested = "reply_requested"; // add to all
    public const string ReplyUri = "reply_uri"; // add to all
    public const string AckRequested = "ack_requested"; // add to all
    public const string ReceivedAt = "received_at"; // add to all

    public static readonly string IncomingFields =
        $"{Body}, {Id}, {Status}, {OwnerId}, {ExecutionTime}, {Attempts}, {CausationId}, {CorrelationId}, {SagaId}, {MessageType}, {ContentType}, {ReplyRequested}, {AckRequested}, {ReplyUri}";

    public static readonly string OutgoingFields =
        $"{Body}, {Id}, {OwnerId}, {Destination}, {DeliverBy}, {Attempts}, {CausationId}, {CorrelationId}, {SagaId}, {MessageType}, {ContentType}, {ReplyRequested}, {AckRequested}, {ReplyUri}";

    public static readonly string DeadLetterFields =
        $"{Id}, {ExecutionTime}, {Attempts}, {Body}, {CausationId}, {CorrelationId}, {SagaId}, {MessageType}, {ContentType}, {ReplyRequested}, {AckRequested}, {ReplyUri}, {Source}, {Explanation}, {ExceptionText}, {ExceptionType}, {ExceptionMessage}";
}
