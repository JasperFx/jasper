using System;
using System.Text;
using Jasper.Runtime.Serialization;
using Newtonsoft.Json;

namespace Jasper.Runtime.Interop.MassTransit;

public class MassTransitJsonSerializer : IMessageSerializer
{
    private readonly IMessageSerializer
        _inner = new SystemTextJsonSerializer(SystemTextJsonSerializer.DefaultOptions());
    private readonly string? _destination;
    private readonly Lazy<string> _reply;

    public MassTransitJsonSerializer(IMassTransitInteropEndpoint endpoint)
    {
        _destination = endpoint.MassTransitUri()?.ToString();
        _reply = new Lazy<string>(() => endpoint.MassTransitReplyUri()?.ToString());
    }

    public string ContentType { get; } = "application/vnd.masstransit+json";
    public byte[] Write(Envelope envelope)
    {
        var message = new MassTransitEnvelope(envelope)
        {
            DestinationAddress = _destination,
            ResponseAddress = _reply.Value
        };

        return _inner.WriteMessage(message);
    }

    public object ReadFromData(Type messageType, Envelope envelope)
    {
        var json = Encoding.Default.GetString(envelope.Data);
        var wrappedType = typeof(MassTransitEnvelope<>).MakeGenericType(messageType);

        var mtEnvelope = (IMassTransitEnvelope)_inner.ReadFromData(wrappedType, envelope);
        mtEnvelope.TransferData(envelope);

        return mtEnvelope.Body!;
    }

    public object ReadFromData(byte[] data)
    {
        // TODO -- IS there a default message type we could use?
        throw new NotSupportedException();
    }

    public byte[] WriteMessage(object message)
    {
        throw new NotSupportedException();
    }
}
