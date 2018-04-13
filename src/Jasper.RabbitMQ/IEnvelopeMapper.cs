using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public interface IEnvelopeMapper
    {
        Envelope ReadEnvelope(BasicDeliverEventArgs args);
        void WriteFromEnvelope(Envelope envelope, IBasicProperties properties);
    }
}
