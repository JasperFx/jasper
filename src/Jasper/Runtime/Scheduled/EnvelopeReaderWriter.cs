using System;
using Baseline;
using Jasper.Serialization;
using Jasper.Transports;

namespace Jasper.Runtime.Scheduled
{
    public class EnvelopeReaderWriter : IMessageSerializer, IMessageDeserializer
    {
        public string MessageType { get; } = TransportConstants.ScheduledEnvelope;
        public Type DotNetType { get; } = typeof(Envelope);
        public string ContentType { get; } = TransportConstants.SerializedEnvelope;
        public static IMessageSerializer Instance { get; } = new EnvelopeReaderWriter();

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
