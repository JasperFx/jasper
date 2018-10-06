using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class consume_a_message_inline : IntegrationContext, IMessageLogger
    {
        private readonly WorkTracker theTracker = new WorkTracker();


        private Task configure()
        {
            return with(_ =>
            {
                _.Services.AddSingleton(theTracker);

                _.Publish.MessagesFromAssemblyContaining<Message1>().To("loopback://cascading");

                _.Services.AddSingleton<IMessageLogger>(this);

                _.Handlers.OnException<DivideByZeroException>().Requeue();
                _.Handlers.DefaultMaximumAttempts = 3;
            });
        }


        void IMessageLogger.Sent(Envelope envelope)
        {
        }

        void IMessageLogger.Received(Envelope envelope)
        {
        }

        void IMessageLogger.ExecutionStarted(Envelope envelope)
        {
        }

        void IMessageLogger.ExecutionFinished(Envelope envelope)
        {
        }

        void IMessageLogger.MessageSucceeded(Envelope envelope)
        {
        }

        public readonly IList<Exception> Exceptions = new List<Exception>();

        void IMessageLogger.MessageFailed(Envelope envelope, Exception ex)
        {
            Exceptions.Add(ex);
        }

        void IMessageLogger.LogException(Exception ex, Guid correlationId, string message)
        {
            Exceptions.Add(ex);
        }

        void IMessageLogger.NoHandlerFor(Envelope envelope)
        {
        }

        void IMessageLogger.NoRoutesFor(Envelope envelope)
        {
        }

        void IMessageLogger.MovedToErrorQueue(Envelope envelope, Exception ex)
        {
        }

        void IMessageLogger.DiscardedEnvelope(Envelope envelope)
        {
        }

        [Fact]
        public async Task exceptions_will_be_thrown_to_caller()
        {
            await configure();

            var message = new Message5
            {
                FailThisManyTimes = 1
            };


            await Exception<DivideByZeroException>
                .ShouldBeThrownByAsync(() => Bus.Invoke(message));
        }

        [Fact]
        public async Task will_log_an_exception()
        {
            await configure();

            try
            {
                await Bus.Invoke(new Message5 {FailThisManyTimes = 1});
            }
            catch (Exception)
            {
            }

            Exceptions.Any().ShouldBeTrue();
        }

        [Fact]
        public async Task will_process_inline()
        {
            await configure();

            var message = new Message5();

            await Bus.Invoke(message);

            theTracker.LastMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task will_send_cascading_messages()
        {
            await configure();

            var message = new Message5();

            await Bus.Invoke(message);

            var m1 = await theTracker.Message1;
            m1.Id.ShouldBe(message.Id);

            var m2 = await theTracker.Message2;
            m2.Id.ShouldBe(message.Id);
        }
    }

    public class WorkTracker
    {
        private readonly TaskCompletionSource<Message1> _message1 = new TaskCompletionSource<Message1>();
        private readonly TaskCompletionSource<Message2> _message2 = new TaskCompletionSource<Message2>();
        public Message5 LastMessage;

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
            if (message.FailThisManyTimes != 0 && message.FailThisManyTimes >= envelope.Attempts)
                throw new DivideByZeroException();

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
