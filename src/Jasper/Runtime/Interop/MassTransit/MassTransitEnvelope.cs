using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Jasper.Runtime.Interop.MassTransit;

internal class MassTransitEnvelope
{
    public MassTransitEnvelope()
    {
    }

    public string? MessageId { get; set; }
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ConversationId { get; set; }
    public string? InitiatorId { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public string? ResponseAddress { get; set; }
    public string? FaultAddress { get; set; }
    public string[]? MessageType { get; set; }

    public object? Body { get; set; }


    public DateTime? ExpirationTime { get; set; }
    public DateTime? SentTime { get; set; }

    public Dictionary<string, object?> Headers { get; set; } = new();

    public void TransferData(Envelope envelope)
    {
        if (MessageId != null && Guid.TryParse(MessageId, out var id))
        {
            envelope.Id = id;
        }

        envelope.CorrelationId = CorrelationId;

        if (ConversationId != null && Guid.TryParse(ConversationId, out var cid))
        {
            envelope.ConversationId = cid;
        }

        foreach (var header in Headers)
        {
            envelope.Headers[header.Key] = header.Value?.ToString();
        }

        if (ExpirationTime.HasValue)
        {
            envelope.DeliverBy = ExpirationTime.Value.ToUniversalTime();
        }

        if (SentTime.HasValue)
        {
            envelope.SentAt = SentTime.Value.ToUniversalTime();
        }
    }
}

[Serializable]
internal class MassTransitEnvelope<T> : MassTransitEnvelope where T : class
{
    public T? Message
    {
        get => Body as T;
        set => Body = value;
    }

    // Jasper doesn't care about this, so don't bother deserializing it
    public BusHostInfo? Host => BusHostInfo.Instance;

}
