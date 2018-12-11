using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkerQueue
    {
        int QueuedCount { get; }

        IScheduledJobProcessor ScheduledJobs { get; }
        Task Enqueue(Envelope envelope);
        void AddQueue(string queueName, int parallelization);
    }
}
