using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Transports.Sending
{
    public class CircuitWatcherTester
    {
        //[Fact]  //TODO -- CI doesn't like this one sometimes.
        public void ping_until_connected()
        {
            var completed = new ManualResetEvent(false);

            var watcher = new CircuitWatcher(new StubCircuit(5, completed), default(CancellationToken));


            completed.WaitOne(1.Seconds())
                .ShouldBeTrue();

        }
    }

    public class StubCircuit : ICircuit
    {
        private readonly int _failureCount;
        private readonly ManualResetEvent _completed;

        public readonly IList<Envelope> Queued = new List<Envelope>();

        private int _count;

        public StubCircuit(int failureCount, ManualResetEvent completed)
        {
            _failureCount = failureCount;
            _completed = completed;
        }

        public void Dispose()
        {
        }

        public void Start(ISenderCallback callback)
        {
        }

        public Task Enqueue(Envelope envelope)
        {
            Queued.Add(envelope);
            return Task.CompletedTask;
        }

        public Uri Destination { get; } = TransportConstants.LocalUri;

        public int QueuedCount => 0;

        public bool Latched { get; private set; }

        public Task LatchAndDrain()
        {
            Latched = false;
            return Task.CompletedTask;
        }

        public void Unlatch()
        {
            Latched = true;
        }

        public Task<bool> TryToResumeAsync(CancellationToken cancellationToken)
        {
            _count++;

            if (_count < _failureCount) throw new Exception("No!");



            return Task.FromResult(true);
        }

        public Task ResumeAsync(CancellationToken cancellationToken)
        {
            _completed.Set();
            return Task.CompletedTask;
        }

        public TimeSpan RetryInterval { get; } = 50.Milliseconds();

        public bool SupportsNativeScheduledSend { get; } = true;
    }
}
