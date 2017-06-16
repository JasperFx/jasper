using System.Threading.Tasks;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;

namespace JasperBus.Tests
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

            var message = new Message5();

            await bus.Consume(message);

            tracker.LastMessage.ShouldBeSameAs(message);
        }
    }

    public class WorkTracker
    {
        public Message5 LastMessage;
    }

    public class WorkConsumer
    {
        private readonly WorkTracker _tracker;

        public WorkConsumer(WorkTracker tracker)
        {
            _tracker = tracker;
        }

        public void Handle(Message5 message)
        {
            _tracker.LastMessage = message;
        }
    }
}