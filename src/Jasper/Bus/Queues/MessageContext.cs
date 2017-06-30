using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues
{
    public class MessageContext
    {
        internal MessageContext(Envelope message, Queue queue)
        {
            Message = message;
            QueueContext = new QueueContext(queue, message);
        }

        public Envelope Message { get; set; }
        public IQueueContext QueueContext { get; }
    }
}
