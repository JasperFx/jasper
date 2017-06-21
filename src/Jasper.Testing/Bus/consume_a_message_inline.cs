using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class consume_a_message_inline : IntegrationContext
    {
        [Fact]
        public async Task will_process_inline()
        {
            var tracker = new WorkTracker();

            with(_ =>
            {
                _.Services.For<WorkTracker>().Use(tracker);
            });


            var bus = Runtime.Container.GetInstance<IServiceBus>();

            var message = new Message5
            {

            };

            await bus.Consume(message);

            tracker.LastMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task will_send_cascading_messages()
        {
            var tracker = new WorkTracker();

            with(_ =>
            {
                _.Services.For<WorkTracker>().Use(tracker);
            });


            var bus = Runtime.Container.GetInstance<IServiceBus>();

            var message = new Message5
            {

            };

            await bus.Consume(message);

            var m1 = await tracker.Message1;
            m1.Id.ShouldBe(message.Id);

            var m2 = await tracker.Message2;
            m2.Id.ShouldBe(message.Id);
        }
    }

    public class WorkTracker
    {
        public Message5 LastMessage;

        private readonly TaskCompletionSource<Message1> _message1 = new TaskCompletionSource<Message1>();
        private readonly TaskCompletionSource<Message2> _message2 = new TaskCompletionSource<Message2>();

        public Task<Message1> Message1 => _message1.Task;
        public Task<Message2> Message2 => _message2.Task;

        public void Record(Message2 message)
        {
            _message2.SetResult(message);
        }

        public void Record(Message1 message)
        {
            _message1.SetResult(message);
        }
    }

    public class WorkConsumer
    {
        private readonly WorkTracker _tracker;

        public WorkConsumer(WorkTracker tracker)
        {
            _tracker = tracker;
        }

        public object[] Handle(Message5 message)
        {
            _tracker.LastMessage = message;

            return new object[] {new Message1 {Id = message.Id}, new Message2 {Id = message.Id}};
        }



        public void Handle(Message2 message)
        {
            _tracker.Record(message);
        }

        public void Handle(Message1 message)
        {
            _tracker.Record(message);
        }
    }
}
