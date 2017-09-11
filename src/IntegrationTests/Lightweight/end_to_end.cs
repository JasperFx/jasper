using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Lightweight
{
    public class end_to_end : IDisposable
    {
        private readonly JasperRuntime theSender;
        private readonly Uri theAddress = "tcp://localhost:2114/incoming".ToUri();
        private readonly MessageTracker theTracker = new MessageTracker();
        private readonly JasperRuntime theReceiver;
        private FakeDelayedJobProcessor delayedJobs;

        public end_to_end()
        {
            theSender = JasperRuntime.For(new JasperRegistry());

            var receiver = new JasperRegistry();
            receiver.Transports.ListenForMessagesFrom(theAddress);
            receiver.ErrorHandling.OnException<DivideByZeroException>().Requeue();
            receiver.ErrorHandling.OnException<TimeoutException>().RetryLater(1.Minutes());

            receiver.Send.Policies.DefaultMaximumAttempts = 3;

            delayedJobs = new FakeDelayedJobProcessor();

            receiver.Send.DelayedProcessing.Use(delayedJobs);

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

            await theSender.Bus.Send(theAddress, new Message1());

            var env = await waiter;

            env.Message.ShouldBeOfType<Message1>();
        }

        [Fact]
        public async Task can_apply_requeue_mechanics()
        {
            var waiter = theTracker.WaitFor<Message2>();

            await theSender.Bus.Send(theAddress, new Message2());

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

        public void Start(IHandlerPipeline pipeline, IChannelGraph channels)
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

    public interface ITracker
    {
        void Check(Envelope envelope, object message);
    }

    public class CountTracker<T> : ITracker
    {
        private readonly int _expected;
        private readonly List<ITracker> _trackers;
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
        private int _count = 0;

        public CountTracker(int expected, List<ITracker> trackers)
        {
            _expected = expected;
            _trackers = trackers;
        }

        public Task<bool> Completion => _completion.Task;
        public void Check(Envelope envelope, object message)
        {
            if (message is T)
            {
                Interlocked.Increment(ref _count);

                if (_count >= _expected)
                {
                    _completion.TrySetResult(true);
                    _trackers.Remove(this);
                }
            }
        }
    }

    public class MessageTracker
    {
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        private readonly ConcurrentBag<ITracker> _trackers = new ConcurrentBag<ITracker>();

        public void Record(object message, Envelope envelope)
        {
            foreach (var tracker in _trackers)
            {
                tracker.Check(envelope, message);
            }

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
