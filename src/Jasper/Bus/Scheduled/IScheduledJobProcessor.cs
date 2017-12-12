using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Scheduled
{
    public interface IScheduledJobProcessor
    {
        void Enqueue(DateTimeOffset executionTime, Envelope envelope);

        Task PlayAll();

        Task PlayAt(DateTime executionTime);

        Task EmptyAll();

        int Count();

        ScheduledJob[] QueuedJobs();

        void Start(IWorkerQueue workerQueue);
    }
}
