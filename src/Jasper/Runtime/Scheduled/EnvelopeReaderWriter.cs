using System;
using Baseline;
using Jasper.Serialization;
using Jasper.Transports;

namespace Jasper.Runtime.Scheduled;

public class EnvelopeReaderWriter : IMessageSerializer
{
    public static IMessageSerializer Instance { get; } = new EnvelopeReaderWriter();
    public string ContentType => TransportConstants.SerializedEnvelope;

    public object ReadFromData(Type messageType, byte[] data)
    {
        if (messageType != typeof(Envelope))
        {
            throw new ArgumentOutOfRangeException(nameof(messageType), "This serializer only supports envelopes");
        }

        return ReadFromData(data);
    }

    public object ReadFromData(byte[] data)
    {
        var envelope = EnvelopeSerializer.Deserialize(data);

        return envelope;
    }

    public byte[] Write(object model)
    {
        return EnvelopeSerializer.Serialize(model.As<Envelope>());
    }
}
