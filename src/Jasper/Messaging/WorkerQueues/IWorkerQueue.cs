using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkerQueue
    {
        Task Enqueue(Envelope envelope);
        int QueuedCount { get; }
        void AddQueue(string queueName, int parallelization);

        IScheduledJobProcessor ScheduledJobs { get; }
    }
}
