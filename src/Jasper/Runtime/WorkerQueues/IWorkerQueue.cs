using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues;

public interface IWorkerQueue : IListeningWorkerQueue
{
    int QueuedCount { get; }

    void Enqueue(Envelope envelope);

    void ScheduleExecution(Envelope envelope);

    void StartListening(IListener listener);
}
