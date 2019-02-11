using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Microsoft.AspNetCore.Http;

namespace Jasper.Messaging.Scheduled
{
    public class EnvelopeReaderWriter : IMessageSerializer, IMessageDeserializer
    {
        public string MessageType { get; } = TransportConstants.ScheduledEnvelope;
        public Type DotNetType { get; } = typeof(Envelope);
        public string ContentType { get; } = TransportConstants.SerializedEnvelope;
        public object ReadFromData(byte[] data)
        {
            return Envelope.Deserialize(data);
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotSupportedException();
        }

        public byte[] Write(object model)
        {
            return model.As<Envelope>().Serialize();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotSupportedException();
        }
    }
}