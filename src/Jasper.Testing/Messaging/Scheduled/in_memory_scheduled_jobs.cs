using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Scheduled
{
    public class in_memory_scheduled_jobs : IWorkerQueue
    {
        private readonly InMemoryScheduledJobProcessor theScheduledJobs;
        private readonly IList<Envelope> sent = new List<Envelope>();
        private readonly Dictionary<Guid, TaskCompletionSource<Envelope>>
            _callbacks = new Dictionary<Guid, TaskCompletionSource<Envelope>>();


        public in_memory_scheduled_jobs()
        {
            theScheduledJobs = new InMemoryScheduledJobProcessor(this);
        }

        Task IWorkerQueue.Enqueue(Envelope envelope)
        {
            sent.Add(envelope);
            if (_callbacks.ContainsKey(envelope.Id))
            {
                _callbacks[envelope.Id].SetResult(envelope);
            }

            return Task.CompletedTask;
        }

        int IWorkerQueue.QueuedCount => 5;

        void IWorkerQueue.AddQueue(string queueName, int parallelization)
        {
            // nothing
        }

        public IScheduledJobProcessor ScheduledJobs => theScheduledJobs;


        public Uri Uri { get; }
        public Uri ReplyUri { get; }
        public Uri Destination { get; } = "loopback://delayed".ToUri();
        public Uri Alias { get; }

        private Task<Envelope> waitForReceipt(Envelope envelope)
        {
            var source = new TaskCompletionSource<Envelope>();
            _callbacks.Add(envelope.Id, source);

            return source.Task;
        }

        [Fact]
        public void run_simplest_case()
        {
            var envelope = ObjectMother.Envelope();
            var waiter = waitForReceipt(envelope);

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), envelope);

            theScheduledJobs.Count().ShouldBe(1);

            waiter.Wait(10.Seconds());

            sent.ShouldContain(envelope);

            theScheduledJobs.Count().ShouldBe(0);
        }

        [Fact]
        public async Task run_multiple_messages_through()
        {
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
        }

        [Fact]
        public async Task play_all()
        {
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
        }



        [Fact]
        public void empty_all()
        {
            var env1 = ObjectMother.Envelope();
            var env2 = ObjectMother.Envelope();
            var env3 = ObjectMother.Envelope();

            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env1);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env2);
            theScheduledJobs.Enqueue(DateTime.UtcNow.AddSeconds(1), env3);

            theScheduledJobs.Count().ShouldBe(3);

            theScheduledJobs.EmptyAll();

            theScheduledJobs.Count().ShouldBe(0);


            Thread.Sleep(2000);

            sent.Any().ShouldBeFalse();
        }


        [Fact]
        public async Task play_at_certain_time()
        {
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

        }

    }
}
