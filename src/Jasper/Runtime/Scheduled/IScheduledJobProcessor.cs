using System;
using System.Threading.Tasks;

namespace Jasper.Runtime.Scheduled;

public interface IScheduledJobProcessor : IDisposable
{
    void Enqueue(DateTimeOffset executionTime, Envelope envelope);

    Task PlayAllAsync();

    Task PlayAtAsync(DateTime executionTime);

    Task EmptyAllAsync();

    int Count();

    ScheduledJob[] QueuedJobs();
}
