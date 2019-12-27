using System;
using System.Threading.Tasks;

namespace Jasper.Runtime.Scheduled
{
    public interface IScheduledJobProcessor : IDisposable
    {
        void Enqueue(DateTimeOffset executionTime, Envelope envelope);

        Task PlayAll();

        Task PlayAt(DateTime executionTime);

        Task EmptyAll();

        int Count();

        ScheduledJob[] QueuedJobs();
    }
}
