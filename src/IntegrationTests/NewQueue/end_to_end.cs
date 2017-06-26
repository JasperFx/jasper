using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace IntegrationTests.NewQueue
{
    public class end_to_end : IDisposable
    {
        private readonly JasperRuntime theSender;
        private readonly Uri theAddress = "jasper://localhost:2114/incoming".ToUri();
        private readonly MessageTracker theTracker;
        private readonly JasperRuntime theReceiver;

        public end_to_end()
        {
            theSender = JasperRuntime.Basic();

            var receiver = new JasperBusRegistry();
            receiver.ListenForMessagesFrom(theAddress);

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
    }

    public class MessageTracker
    {
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

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
