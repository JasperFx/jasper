using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public interface IEnvelopeMapper
    {
        Envelope From(BasicDeliverEventArgs args);
        void Apply(Envelope envelope, IBasicProperties properties);
    }
}