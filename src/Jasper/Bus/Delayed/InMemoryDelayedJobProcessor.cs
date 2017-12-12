using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Delayed
{
    public class InMemoryDelayedJobProcessor : IDelayedJobProcessor, IDisposable
    {
        private readonly ConcurrentCache<Guid, InMemoryDelayedJob> _outstandingJobs
            = new ConcurrentCache<Guid, InMemoryDelayedJob>();

        public IWorkerQueue _queue;


        public void Enqueue(DateTimeOffset executionTime, Envelope envelope)
        {
            _outstandingJobs[envelope.Id] = new InMemoryDelayedJob(this, envelope, executionTime);
        }

        public void Start(IWorkerQueue workerQueue)
        {
            _queue = workerQueue;
        }

        public async Task PlayAll()
        {
            var outstanding = _outstandingJobs.ToArray();
            foreach (var job in outstanding)
                await job.Enqueue();
        }

        public async Task PlayAt(DateTime executionTime)
        {
            var outstanding = _outstandingJobs.Where(x => x.ExecutionTime <= executionTime).ToArray();
            foreach (var job in outstanding)
                await job.Enqueue();
        }

        public Task EmptyAll()
        {
            var outstanding = _outstandingJobs.ToArray();
            foreach (var job in outstanding)
                job.Cancel();

            return Task.CompletedTask;
        }

        public int Count()
        {
            return _outstandingJobs.Count;
        }

        public DelayedJob[] QueuedJobs()
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

        public static InMemoryDelayedJobProcessor ForQueue(IWorkerQueue queue)
        {
            return new InMemoryDelayedJobProcessor {_queue = queue};
        }

        public class InMemoryDelayedJob
        {
            private readonly CancellationTokenSource _cancellation;
            private readonly InMemoryDelayedJobProcessor _parent;
            private Task _task;

            public InMemoryDelayedJob(InMemoryDelayedJobProcessor parent, Envelope envelope, DateTimeOffset executionTime)
            {
                _parent = parent;
                ExecutionTime = executionTime.ToUniversalTime();
                envelope.ExecutionTime = null;

                Envelope = envelope;

                _cancellation = new CancellationTokenSource();
                var delayTime = ExecutionTime.Subtract(DateTime.UtcNow);
                _task = Task.Delay(delayTime, _cancellation.Token).ContinueWith(publish);
                ReceivedAt = DateTime.UtcNow;
            }

            public DateTimeOffset ExecutionTime { get; }

            public DateTime ReceivedAt { get; }

            public Envelope Envelope { get; }

            private Task publish(Task obj)
            {
                return _cancellation.IsCancellationRequested
                    ? Task.CompletedTask
                    : Enqueue();
            }

            public void Cancel()
            {
                _cancellation.Cancel();
                _parent._outstandingJobs.Remove(Envelope.Id);
            }

            public DelayedJob ToReport()
            {
                return new DelayedJob(Envelope.Id)
                {
                    ExecutionTime = ExecutionTime,
                    From = Envelope.ReceivedAt.ToString(),
                    ReceivedAt = ReceivedAt,
                    MessageType = Envelope.MessageType
                };
            }

            public async Task Enqueue()
            {
                Envelope.Callback = new LightweightCallback(_parent._queue);
                await _parent._queue.Enqueue(Envelope);
                Cancel();
            }
        }
    }
}
