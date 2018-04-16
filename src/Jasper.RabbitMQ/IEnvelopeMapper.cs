using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public interface IEnvelopeMapper
    {
        void WriteFromEnvelope(Envelope envelope, IBasicProperties properties);
        Envelope ReadEnvelope(byte[] body, IBasicProperties properties);
    }
}
