using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues
{
    public interface IWorkerQueue : IListeningWorkerQueue
    {
        int QueuedCount { get; }

        Task EnqueueAsync(Envelope envelope);

        Task ScheduleExecutionAsync(Envelope envelope);

        void StartListening(IListener listener);
    }


}
