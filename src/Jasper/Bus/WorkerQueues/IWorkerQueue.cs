using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Scheduled;

namespace Jasper.Bus.WorkerQueues
{
    public interface IWorkerQueue
    {
        Task Enqueue(Envelope envelope);
        int QueuedCount { get; }
        void AddQueue(string queueName, int parallelization);

        IScheduledJobProcessor ScheduledJobs { get; }
    }
}
