using System;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Scheduled
{
    public class EnvelopeReaderWriter : IMessageSerializer, IMessageDeserializer
    {
        public string MessageType { get; } = TransportConstants.ScheduledEnvelope;
        public Type DotNetType { get; } = typeof(Envelope);
        public string ContentType { get; } = TransportConstants.SerializedEnvelope;
        public static IMessageSerializer Instance { get; } = new EnvelopeReaderWriter();

        public object ReadFromData(byte[] data)
        {
            return Envelope.Deserialize(data);
        }

        public byte[] Write(object model)
        {
            return model.As<Envelope>().Serialize();
        }

    }
}
