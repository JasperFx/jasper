using System.Threading.Tasks;

namespace Jasper.Runtime
{
    public interface IQueuedOutgoingMessages
    {
        Task SendAllQueuedOutgoingMessages();
        void AbortAllQueuedOutgoingMessages();
    }
}
