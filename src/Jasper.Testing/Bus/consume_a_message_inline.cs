using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class consume_a_message_inline : IntegrationContext, IBusLogger
    {
        private readonly WorkTracker theTracker = new WorkTracker();


        public consume_a_message_inline()
        {
            with(_ =>
            {
                _.Services.For<WorkTracker>().Use(theTracker);

                _.SendMessagesFromAssemblyContaining<Message1>().To("memory://cascading");

                _.Logging.LogBusEventsWith(this);

                _.ErrorHandling.OnException<DivideByZeroException>().Requeue();
                _.Policies.DefaultMaximumAttempts = 3;
            });

            
        }

        [Fact]
        public async Task will_process_inline()
        {
            var message = new Message5
            {

            };

            await Bus.Consume(message);

            theTracker.LastMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task will_send_cascading_messages()
        {
            var message = new Message5
            {

            };

            await Bus.Consume(message);

            var m1 = await theTracker.Message1;
            m1.Id.ShouldBe(message.Id);

            var m2 = await theTracker.Message2;
            m2.Id.ShouldBe(message.Id);
        }

        [Fact]
        public async Task will_log_an_exception()
        {
            await Bus.Consume(new Message5 {FailThisManyTimes = 1});

            Exceptions.Any().ShouldBeTrue();
        }


        void IBusLogger.Sent(Envelope envelope)
        {

        }

        void IBusLogger.Received(Envelope envelope)
        {
        }

        void IBusLogger.ExecutionStarted(Envelope envelope)
        {
        }

        void IBusLogger.ExecutionFinished(Envelope envelope)
        {
        }

        void IBusLogger.MessageSucceeded(Envelope envelope)
        {
        }

        public readonly IList<Exception> Exceptions = new List<Exception>();

        void IBusLogger.MessageFailed(Envelope envelope, Exception ex)
        {
            Exceptions.Add(ex);
        }

        void IBusLogger.LogException(Exception ex, string correlationId, string message)
        {
            Exceptions.Add(ex);
        }

        void IBusLogger.NoHandlerFor(Envelope envelope)
        {
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

        public object[] Handle(Envelope envelope, Message5 message)
        {
            if (message.FailThisManyTimes >= envelope.Attempts)
            {
                throw new DivideByZeroException();
            }

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
