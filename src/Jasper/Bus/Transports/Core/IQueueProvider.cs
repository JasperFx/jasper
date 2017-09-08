using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Core
{
    public interface IQueueProvider
    {
        IMessageCallback BuildCallback(Envelope envelope, QueueReceiver receiver);

        // TODO -- make this async
        void StoreIncomingMessages(Envelope[] messages);

        void RemoveIncomingMessages(Envelope[] messages);
    }
}
