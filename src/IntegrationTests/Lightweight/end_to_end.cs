using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace IntegrationTests.Lightweight
{
    public class end_to_end : IDisposable
    {
        private readonly JasperRuntime theSender;
        private readonly Uri theAddress = "jasper://localhost:2114/incoming".ToUri();
        private readonly MessageTracker theTracker;
        private readonly JasperRuntime theReceiver;
        private FakeDelayedJobProcessor delayedJobs;

        public end_to_end()
        {
            theSender = JasperRuntime.For(new JasperBusRegistry());

            var receiver = new JasperBusRegistry();
            receiver.ListenForMessagesFrom(theAddress);
            receiver.ErrorHandling.OnException<DivideByZeroException>().Requeue();
            receiver.ErrorHandling.OnException<TimeoutException>().RetryLater(1.Minutes());

            receiver.Policies.DefaultMaximumAttempts = 3;

            delayedJobs = new FakeDelayedJobProcessor();

            receiver.DelayedJobs.Use(delayedJobs);

            theTracker = new MessageTracker();
            receiver.Services.For<MessageTracker>().Use(theTracker);

            theReceiver = JasperRuntime.For(receiver);
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [Fact]
        public async Task can_send_from_one_node_to_another()
        {
            var waiter = theTracker.WaitFor<Message1>();

            await theSender.Container.GetInstance<IServiceBus>().Send(theAddress, new Message1());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message1>();
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {
            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Container.GetInstance<IServiceBus>().Send(theAddress, new Message2());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message2>();
        }

        [Fact]
        public async Task delayed_processor_mechanics()
        {
            await theSender.Container.GetInstance<IServiceBus>().Send(theAddress, new TimeoutsMessage());

            var envelope = await delayedJobs.Envelope();

            envelope.Message.ShouldBeOfType<TimeoutsMessage>();
        }
    }


    public class TimeoutsMessage
    {

    }

    public class FakeDelayedJobProcessor : IDelayedJobProcessor
    {
        private readonly TaskCompletionSource<Envelope> _envelope = new TaskCompletionSource<Envelope>();

        public Task<Envelope> Envelope()
        {
            return _envelope.Task;
        }

        public void Enqueue(DateTime executionTime, Envelope envelope)
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

        public DelayedJob[] QueuedJobs()
        {
            throw new NotImplementedException();
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
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

    public class MessageTracker
    {
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        public MessageTracker()
        {
            Console.WriteLine("Making me");
        }

        public void Record(object message, Envelope envelope)
        {
            var messageType = message.GetType();
            var list = _waiters[messageType];

            list.Each(x => x.SetResult(envelope));

            list.Clear();
        }

        public Task<Envelope> WaitFor<T>()
        {
            var source = new TaskCompletionSource<Envelope>();
            _waiters[typeof(T)].Add(source);

            return source.Task;
        }
    }
}
