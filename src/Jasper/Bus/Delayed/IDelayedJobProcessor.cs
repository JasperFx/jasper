using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Delayed
{
    public interface IDelayedJobProcessor
    {
        void Enqueue(DateTimeOffset executionTime, Envelope envelope);

        Task PlayAll();

        Task PlayAt(DateTime executionTime);

        Task EmptyAll();

        int Count();

        DelayedJob[] QueuedJobs();

        void Start(IWorkerQueue workerQueue);
    }
}
