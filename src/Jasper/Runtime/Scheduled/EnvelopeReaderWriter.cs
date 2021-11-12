using System;
using Baseline;
using Jasper.Serialization;
using Jasper.Serialization.New;
using Jasper.Transports;

namespace Jasper.Runtime.Scheduled
{
    public class EnvelopeReaderWriter : INewSerializer
    {
        public string ContentType { get; } = TransportConstants.SerializedEnvelope;
        public static INewSerializer Instance { get; } = new EnvelopeReaderWriter();

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
