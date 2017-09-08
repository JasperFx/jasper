using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.Durable
{
    public class DurableQueueProvider : IQueueProvider
    {
        private readonly IPersistence _persistence;

        public DurableQueueProvider(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public IMessageCallback BuildCallback(Envelope envelope, QueueReceiver receiver)
        {
            return new DurableCallback(receiver.QueueName, envelope, _persistence, receiver.Enqueue);
        }

        public void StoreIncomingMessages(Envelope[] messages)
        {
            _persistence.StoreInitial(messages);
        }

        public void RemoveIncomingMessages(Envelope[] messages)
        {
            _persistence.Remove(messages);
        }
    }
}
