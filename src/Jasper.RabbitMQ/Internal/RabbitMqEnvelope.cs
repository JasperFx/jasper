using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEnvelope : Envelope
    {
        internal IModel Model { get; }
        internal ulong DeliveryTag { get; }
        internal RabbitMqSender RabbitSender { get; }

        public RabbitMqEnvelope(IModel model, ulong deliveryTag, RabbitMqSender rabbitSender) : base()
        {
            Model = model;
            DeliveryTag = deliveryTag;
            RabbitSender = rabbitSender;
        }

        internal void Complete()
        {
            Model.BasicAck(DeliveryTag, false);
        }

        internal Task Defer()
        {
            // TODO -- how can you do this transactionally?
            Model.BasicNack(DeliveryTag, false, true);
            return RabbitSender.Send(this);
        }


    }
}
