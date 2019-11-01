using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkerQueue
    {
        int QueuedCount { get; }

        [Obsolete("Delete with GH-557")]
        IScheduledJobProcessor ScheduledJobs { get; }
        Task Enqueue(Envelope envelope);

        [Obsolete("Delete with GH-557")]
        void AddQueue(string queueName, int parallelization);

        Task ScheduleExecution(Envelope envelope);
        Task StoreIncoming(Envelope[] envelopes);
    }


}
