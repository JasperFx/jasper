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
            Listener = null;
            Sender = null;
        }

        internal async Task Defer()
        {
            await Listener.Requeue(this);

            Listener = null;
            Sender = null;
        }


    }
}
