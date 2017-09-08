using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Loopback;

namespace Jasper.Bus.Delayed
{
    public class InMemoryDelayedJobProcessor : IDelayedJobProcessor, IDisposable
    {
        public static InMemoryDelayedJobProcessor ForChannel(IChannel channel)
        {
            return new InMemoryDelayedJobProcessor{_channel = channel};
        }

        private IChannel _channel;

        private readonly ConcurrentCache<string, InMemoryDelayedJob> _outstandingJobs
            = new ConcurrentCache<string, InMemoryDelayedJob>();


        public void Enqueue(DateTime executionTime, Envelope envelope)
        {
            _outstandingJobs[envelope.CorrelationId] = new InMemoryDelayedJob(this, envelope, executionTime);
        }

        public void Start(IHandlerPipeline pipeline, IChannelGraph channels)
        {
            _channel = channels[LoopbackTransport.Delayed];
        }

        public async Task PlayAll()
        {
            var outstanding = _outstandingJobs.ToArray();
            foreach (var job in outstanding)
            {
                await _channel.Send(job.Envelope);
                job.Cancel();
            }
        }

        public async Task PlayAt(DateTime executionTime)
        {
            var outstanding = _outstandingJobs.Where(x => x.ExecutionTime <= executionTime).ToArray();
            foreach (var job in outstanding)
            {
                await _channel.Send(job.Envelope);
                job.Cancel();
            }
        }

        public Task EmptyAll()
        {
            var outstanding = _outstandingJobs.ToArray();
            foreach (var job in outstanding)
            {
                job.Cancel();
            }

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

        public class InMemoryDelayedJob
        {
            private readonly InMemoryDelayedJobProcessor _parent;
            private readonly CancellationTokenSource _cancellation;
            private Task _task;
            public DateTime ExecutionTime { get; }

            public InMemoryDelayedJob(InMemoryDelayedJobProcessor parent, Envelope envelope, DateTime executionTime)
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

            public DateTime ReceivedAt { get; }

            private Task publish(Task obj)
            {
                return _cancellation.IsCancellationRequested
                    ? Task.CompletedTask
                    : _parent._channel.Send(Envelope).ContinueWith(t => Cancel());
            }

            public void Cancel()
            {
                _cancellation.Cancel();
                _parent._outstandingJobs.Remove(Envelope.CorrelationId);
            }

            public Envelope Envelope { get; }

            public DelayedJob ToReport()
            {
                return new DelayedJob(Envelope.CorrelationId)
                {
                    ExecutionTime = ExecutionTime, From = Envelope.ReceivedAt.ToString(), ReceivedAt = ReceivedAt, MessageType = Envelope.MessageType
                };
            }
        }

        public void Dispose()
        {
            var outstanding = _outstandingJobs.ToArray();
            foreach (var job in outstanding)
            {
                job.Cancel();
            }

            _outstandingJobs.ClearAll();
        }
    }
}
