using System;
using Baseline;
using Jasper.Serialization;
using Jasper.Transports;

namespace Jasper.Runtime.Scheduled
{
    public class EnvelopeReaderWriter : IMessageSerializer
    {
        public string ContentType { get; } = TransportConstants.SerializedEnvelope;
        public static IMessageSerializer Instance { get; } = new EnvelopeReaderWriter();

        public object ReadFromData(Type messageType, byte[] data)
        {
            if (messageType != typeof(Envelope))
                throw new ArgumentOutOfRangeException("This serializer only supports envelopes");
            return ReadFromData(data);
        }

        public object ReadFromData(byte[] data)
        {
            return EnvelopeSerializer.Deserialize(data);
        }

        public byte[] Write(object model)
        {
            return EnvelopeSerializer.Serialize(model.As<Envelope>());
        }

    }
}
