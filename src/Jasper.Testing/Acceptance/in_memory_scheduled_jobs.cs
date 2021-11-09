using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Runtime.Scheduled;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Acceptance
{

    public class in_memory_scheduled_jobs : IAsyncLifetime
    {
        private IHost theHost;
        private ScheduledMessageReceiver theReceiver;

        public async Task InitializeAsync()
        {
            var registry = new ScheduledMessageApp();
            theReceiver = registry.Receiver;

            theHost = await Host.CreateDefaultBuilder().UseJasper(registry).StartAsync();
        }

        public Task DisposeAsync()
        {
            return theHost.StopAsync();
        }

        public Task ScheduleMessage(int id, int seconds)
        {
            return theHost.Get<IMessagePublisher>().Schedule(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        public Task ScheduleSendMessage(int id, int seconds)
        {
            return theHost.Get<IMessagePublisher>().ScheduleSend(new ScheduledMessage {Id = id}, seconds.Seconds());
        }

        public int ReceivedMessageCount()
        {
            return theReceiver.ReceivedMessages.Count;
        }

        public Task AfterReceivingMessages()
        {
            return theReceiver.Received;
        }

        public int TheIdOfTheOnlyReceivedMessage()
        {
            return theReceiver.ReceivedMessages.Single().Id;
        }

        [Fact]
        public async Task run_scheduled_jobs_locally()
        {
            await ScheduleMessage(1, 7200);
            await ScheduleMessage(2, 5);
            await ScheduleMessage(3, 7200);
            ReceivedMessageCount().ShouldBe(0);

            await AfterReceivingMessages();

            TheIdOfTheOnlyReceivedMessage().ShouldBe(2);
        }
    }

    public class ScheduledMessageApp : JasperOptions
    {
        public readonly ScheduledMessageReceiver Receiver = new ScheduledMessageReceiver();

        public ScheduledMessageApp()
        {
            Services.AddSingleton(Receiver);

            Endpoints.Publish(x => x.MessagesFromAssemblyContaining<ScheduledMessageApp>()
                .ToLocalQueue("incoming"));

            Endpoints.ListenForMessagesFrom("local://incoming");

            Handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<ScheduledMessageCatcher>();
            });
        }
    }

    public class ScheduledMessage
    {
        public int Id { get; set; }
    }


    public class ScheduledMessageReceiver
    {
        public readonly IList<ScheduledMessage> ReceivedMessages = new List<ScheduledMessage>();

        public readonly TaskCompletionSource<ScheduledMessage> Source = new TaskCompletionSource<ScheduledMessage>();

        public Task<ScheduledMessage> Received => Source.Task;
    }

    public class ScheduledMessageCatcher
    {
        private readonly ScheduledMessageReceiver _receiver;

        public ScheduledMessageCatcher(ScheduledMessageReceiver receiver)
        {
            _receiver = receiver;
        }


        public void Consume(ScheduledMessage message)
        {
            _receiver.Source.SetResult(message);

            _receiver.ReceivedMessages.Add(message);
        }
    }

    public class InMemoryScheduledJobFixture : IWorkerQueue
    {
        private readonly Dictionary<Guid, TaskCompletionSource<Envelope>>
            _callbacks = new Dictionary<Guid, TaskCompletionSource<Envelope>>();

        private readonly IList<Envelope> sent = new List<Envelope>();
        private InMemoryScheduledJobProcessor theScheduledJobs;

        public Uri Uri { get; }
        public Uri ReplyUri { get; }
        public Uri Destination { get; } = "local://delayed".ToUri();
        public Uri Alias { get; }

        public InMemoryScheduledJobFixture()
        {
            theScheduledJobs = new InMemoryScheduledJobProcessor(this);
            sent.Clear();
            _callbacks.Clear();
        }

        Task IWorkerQueue.Enqueue(Envelope envelope)
        {
            sent.Add(envelope);
            if (_callbacks.ContainsKey(envelope.Id)) _callbacks[envelope.Id].SetResult(envelope);

            return Task.CompletedTask;
        }

        int IWorkerQueue.QueuedCount => 5;

        public Task ScheduleExecution(Envelope envelope)
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

        }


        private Task<Envelope> waitForReceipt(Envelope envelope)
        {
            var source = new TaskCompletionSource<Envelope>();
            _callbacks.Add(envelope.Id, source);

            return source.Task;
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
        public async Task<bool> empty_all()
        {
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


        [Fact]
        public async Task<bool> play_at_certain_time()
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
