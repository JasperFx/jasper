using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Lightweight
{
    [Collection("integration")]
    public class end_to_end : IDisposable
    {
        private static int port = 2114;

        private JasperRuntime theSender;
        private readonly Uri theAddress = $"tcp://localhost:{++port}/incoming".ToUri();
        private readonly MessageTracker theTracker = new MessageTracker();
        private JasperRuntime theReceiver;
        private FakeScheduledJobProcessor scheduledJobs;


        private async Task getReady()
        {
            theSender = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Services.AddSingleton(theTracker);
            });

            var receiver = new JasperRegistry();
            receiver.Handlers.DisableConventionalDiscovery();

            receiver.Transports.ListenForMessagesFrom(theAddress);
            receiver.Handlers.OnException<DivideByZeroException>().Requeue();
            receiver.Handlers.OnException<TimeoutException>().RetryLater(10.Seconds());

            receiver.Handlers.DefaultMaximumAttempts = 3;
            receiver.Handlers.IncludeType<MessageConsumer>();

            scheduledJobs = new FakeScheduledJobProcessor();

            receiver.Services.For<IScheduledJobProcessor>().Use(scheduledJobs);

            receiver.Services.For<MessageTracker>().Use(theTracker);

            theReceiver = await JasperRuntime.ForAsync(receiver);

        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [Fact]
        public async Task can_send_from_one_node_to_another()
        {
            await getReady();

            var waiter = theTracker.WaitFor<Message1>();

            await theSender.Messaging.Send(theAddress, new Message1());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message1>();
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {
            await getReady();

            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Messaging.Send(theAddress, new Message2());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message2>();
        }

        [Fact]
        public async Task tags_the_envelope_with_the_source()
        {
            await getReady();

            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Messaging.Send(theAddress, new Message2());

            var env = await waiter;

            env.Source.ShouldBe(theSender.Get<MessagingSettings>().NodeId);
        }
    }


    public class TimeoutsMessage
    {

    }

    public class FakeScheduledJobProcessor : IScheduledJobProcessor
    {
        private readonly TaskCompletionSource<Envelope> _envelope = new TaskCompletionSource<Envelope>();

        public Task<Envelope> Envelope()
        {
            return _envelope.Task;
        }

        public void Enqueue(DateTimeOffset executionTime, Envelope envelope)
        {
            envelope.ExecutionTime = executionTime;
            _envelope.SetResult(envelope);
        }

        public Task PlayAll()
        {
            throw new NotImplementedException();
        }

        public Task PlayAt(DateTime executionTime)
        {
            throw new NotImplementedException();
        }

        public Task EmptyAll()
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public ScheduledJob[] QueuedJobs()
        {
            throw new NotImplementedException();
        }

        public void Start(IWorkerQueue workerQueue)
        {

        }

        public void Dispose()
        {
        }
    }

    public class MessageConsumer
    {
        private readonly MessageTracker _tracker;

        public MessageConsumer(MessageTracker tracker)
        {
            _tracker = tracker;
        }

        public void Consume(Envelope envelope, Message1 message)
        {
            _tracker.Record(message, envelope);
        }

        public void Consume(Envelope envelope, Message2 message)
        {
            if (envelope.Attempts < 2)
            {
                throw new DivideByZeroException();
            }

            _tracker.Record(message, envelope);
        }

        public void Consume(Envelope envelope, TimeoutsMessage message)
        {
            if (envelope.Attempts < 2)
            {
                throw new TimeoutException();
            }

            _tracker.Record(message, envelope);
        }
    }


}
