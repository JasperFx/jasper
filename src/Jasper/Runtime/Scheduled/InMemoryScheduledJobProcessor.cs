using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime.WorkerQueues;

namespace Jasper.Runtime.Scheduled;

public class InMemoryScheduledJobProcessor : IScheduledJobProcessor
{
    private readonly Cache<Guid, InMemoryScheduledJob> _outstandingJobs = new();

    private readonly IWorkerQueue _queue;

    public InMemoryScheduledJobProcessor(IWorkerQueue queue)
    {
        _queue = queue;
    }

    public void Enqueue(DateTimeOffset executionTime, Envelope envelope)
    {
        _outstandingJobs[envelope.Id] = new InMemoryScheduledJob(this, envelope, executionTime);
    }

    public async Task PlayAllAsync()
    {
        var outstanding = _outstandingJobs.ToArray();
        foreach (var job in outstanding) await job.EnqueueAsync();
    }

    public async Task PlayAtAsync(DateTime executionTime)
    {
        var outstanding = _outstandingJobs.Where(x => x.ExecutionTime <= executionTime).ToArray();
        foreach (var job in outstanding) await job.EnqueueAsync();
    }

    public Task EmptyAllAsync()
    {
        var outstanding = _outstandingJobs.ToArray();
        foreach (var job in outstanding) job.Cancel();

        return Task.CompletedTask;
    }

    public int Count()
    {
        return _outstandingJobs.Count;
    }

    public ScheduledJob[] QueuedJobs()
    {
        return _outstandingJobs.ToArray().Select(x => x.ToReport()).ToArray();
    }

    public void Dispose()
    {
        var outstanding = _outstandingJobs.ToArray();
        foreach (var job in outstanding)
            job.Cancel();

        _outstandingJobs.ClearAll();
    }

    public class InMemoryScheduledJob : IDisposable
    {
        private readonly CancellationTokenSource _cancellation;
        private readonly InMemoryScheduledJobProcessor _parent;
        private Task _task;

        public InMemoryScheduledJob(InMemoryScheduledJobProcessor parent, Envelope envelope,
            DateTimeOffset executionTime)
        {
            _parent = parent;
            ExecutionTime = executionTime.ToUniversalTime();
            envelope.ScheduledTime = null;

            Envelope = envelope;

            _cancellation = new CancellationTokenSource();
            var delayTime = ExecutionTime.Subtract(DateTime.UtcNow);
            _task = Task.Delay(delayTime, _cancellation.Token).ContinueWith(publishAsync, TaskScheduler.Default);

            ReceivedAt = DateTime.UtcNow;
        }

        public DateTimeOffset ExecutionTime { get; }

        public DateTime ReceivedAt { get; }

        public Envelope Envelope { get; }

        private Task publishAsync(Task obj)
        {
            return _cancellation.IsCancellationRequested
                ? Task.CompletedTask
                : EnqueueAsync();
        }

        public void Cancel()
        {
            _cancellation.Cancel();
            _parent._outstandingJobs.Remove(Envelope.Id);
        }

        public ScheduledJob ToReport()
        {
            return new ScheduledJob(Envelope.Id)
            {
                ExecutionTime = ExecutionTime,
                ReceivedAt = ReceivedAt,
                MessageType = Envelope.MessageType
            };
        }

        public async Task EnqueueAsync()
        {
            await _parent._queue.EnqueueAsync(Envelope);
            Cancel();
        }

        public void Dispose()
        {
            _task.Dispose();
        }
    }
}
