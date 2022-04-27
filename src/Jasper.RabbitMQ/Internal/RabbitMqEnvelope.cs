using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEnvelope : Envelope
    {
        internal RabbitMqListener Listener { get; private set; }
        internal ulong DeliveryTag { get; }

        public RabbitMqEnvelope(RabbitMqListener listener, ulong deliveryTag) : base()
        {
            Listener = listener;
            DeliveryTag = deliveryTag;
        }

        internal void Complete()
        {
            Listener.Complete(DeliveryTag);
            Acked = true;
        }

        internal ValueTask Defer()
        {
            Acked = true;
            return Listener.Requeue(this);
        }

        public bool Acked { get; private set; }
    }
}
