using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues
{
    public interface IWorkerQueue : IListeningWorkerQueue
    {
        int QueuedCount { get; }

        Task Enqueue(Envelope envelope);

        Task ScheduleExecution(Envelope envelope);

        void StartListening(IListener listener);
    }


}
