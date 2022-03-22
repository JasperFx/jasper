using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Runtime.Scheduled;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Jasper.Util;
using Shouldly;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public class InMemoryScheduledJobFixture : Fixture, IWorkerQueue
    {
        private readonly Dictionary<Guid, TaskCompletionSource<Envelope>>
            _callbacks = new Dictionary<Guid, TaskCompletionSource<Envelope>>();

        private readonly IList<Envelope> sent = new List<Envelope>();
        private InMemoryScheduledJobProcessor theScheduledJobs;


        public InMemoryScheduledJobFixture()
        {
            Title = "In Memory Scheduled Jobs Compliance";
        }

        public Uri Uri { get; }
        public Uri ReplyUri { get; }
        public Uri Destination { get; } = "local://delayed".ToUri();
        public Uri Alias { get; }

        Task IWorkerQueue.EnqueueAsync(Envelope envelope)
        {
            sent.Add(envelope);
            if (_callbacks.ContainsKey(envelope.Id)) _callbacks[envelope.Id].SetResult(envelope);

            return Task.CompletedTask;
        }

        int IWorkerQueue.QueuedCount => 5;

        public Task ScheduleExecutionAsync(Envelope envelope)
        {
            theScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }

        void IWorkerQueue.StartListening(IListener listener)
        {
            throw new NotImplementedException();
        }

        Task IListeningWorkerQueue.Received(Uri uri, Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public Task Received(Uri uri, Envelope envelope)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }




        public override void SetUp()
        {
            theScheduledJobs = new InMemoryScheduledJobProcessor(this);
            sent.Clear();
            _callbacks.Clear();
        }

        private Task<Envelope> waitForReceipt(Envelope envelope)
        {
            var source = new TaskCompletionSource<Envelope>();
            _callbacks.Add(envelope.Id, source);

            return source.Task;
        }


        [FormatAs("Run multiple messages through the in memory scheduler")]
        public async Task<bool> run_multiple_messages_through()
        {
            SetUp();

            var env1 = ObjectMother.Envelope();
            var env2 = ObjectMother.Envelope();
            var env3 = ObjectMother.Envelope();

            var waiter1 = waitForReceipt(env1);
            var waiter2 = waitForReceipt(env2);
            var waiter3 = waitForReceipt(env3);

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddHours(1), env1);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(5), env2);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddHours(1), env3);

            await waiter2;

            waiter1.IsCompleted.ShouldBeFalse();
            waiter2.IsCompleted.ShouldBeTrue();
            waiter3.IsCompleted.ShouldBeFalse();

            return true;
        }

        [FormatAs("Play All Expored Jobs")]
        public async Task<bool> play_all()
        {
            SetUp();

            var env1 = ObjectMother.Envelope();
            var env2 = ObjectMother.Envelope();
            var env3 = ObjectMother.Envelope();

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddMinutes(1), env1);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddMinutes(1), env2);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddMinutes(1), env3);

            theScheduledJobs.Count().ShouldBe(3);

            await theScheduledJobs.PlayAll();

            theScheduledJobs.Count().ShouldBe(0);
            sent.Count.ShouldBe(3);
            sent.ShouldContain(env1);
            sent.ShouldContain(env2);
            sent.ShouldContain(env3);

            return true;
        }


        [FormatAs("Empty all queued jobs")]
        public async Task<bool> empty_all()
        {
            SetUp();

            var env1 = ObjectMother.Envelope();
            var env2 = ObjectMother.Envelope();
            var env3 = ObjectMother.Envelope();

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env1);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env2);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env3);

            theScheduledJobs.Count().ShouldBe(3);

            await theScheduledJobs.EmptyAll();

            theScheduledJobs.Count().ShouldBe(0);


            await Task.Delay(2000.Milliseconds());

            sent.Any().ShouldBeFalse();

            return true;
        }


        [FormatAs("Play scheduled jobs at a given time")]
        public async Task<bool> play_at_certain_time()
        {
            SetUp();

            var env1 = ObjectMother.Envelope();
            var env2 = ObjectMother.Envelope();
            var env3 = ObjectMother.Envelope();

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddHours(1), env1);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddHours(2), env2);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddHours(3), env3);

            await theScheduledJobs.PlayAt(DateTime.UtcNow.AddMinutes(150));

            sent.Count.ShouldBe(2);
            sent.ShouldContain(env1);
            sent.ShouldContain(env2);
            sent.ShouldNotContain(env3);

            theScheduledJobs.Count().ShouldBe(1);

            return true;
        }
    }

    public static class ObjectMother
    {
        public static Envelope Envelope()
        {
            return new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                MessageType = "Something",
                Destination = TransportConstants.ScheduledUri
            };
        }
    }
}
