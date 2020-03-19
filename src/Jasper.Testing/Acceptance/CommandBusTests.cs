using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class CommandBusTests : IntegrationContext, IMessageLogger
    {
        private readonly WorkTracker theTracker = new WorkTracker();


        public CommandBusTests(DefaultApp @default) : base(@default)
        {
        }

        private void configure()
        {
            with(_ =>
            {
                _.Services.AddSingleton(theTracker);

                _.Endpoints.Publish(x => x.MessagesFromAssemblyContaining<Message1>()
                    .ToLocalQueue("cascading"));

                _.Services.AddSingleton<IMessageLogger>(this);

                _.Handlers.OnException<DivideByZeroException>().Requeue();

                _.Handlers.Retries.MaximumAttempts = 3;
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
        public async Task enqueue_locally()
        {
            var message = new Message1
            {
                Id = Guid.NewGuid()
            };

            var session = await Host.ExecuteAndWait(c => c.Enqueue(message));

            var tracked = session.FindSingleTrackedMessageOfType<Message1>();

            tracked.Id.ShouldBe(message.Id);

        }

        [Fact]
        public async Task exceptions_will_be_thrown_to_caller()
        {
            configure();

            var message = new Message5
            {
                FailThisManyTimes = 1
            };


            await Should.ThrowAsync<DivideByZeroException>(() => Publisher.Invoke(message));
        }

        [Fact]
        public async Task will_log_an_exception()
        {
            configure();

            try
            {
                await Publisher.Invoke(new Message5 {FailThisManyTimes = 1});
            }
            catch (Exception)
            {
            }

            Exceptions.Any().ShouldBeTrue();
        }

        [Fact]
        public async Task will_process_inline()
        {
            configure();

            var message = new Message5();

            await Publisher.Invoke(message);

            theTracker.LastMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task will_send_cascading_messages()
        {
            configure();

            var message = new Message5();

            await Publisher.Invoke(message);

            var m1 = await theTracker.Message1;
            m1.Id.ShouldBe(message.Id);

            var m2 = await theTracker.Message2;
            m2.Id.ShouldBe(message.Id);
        }



        // SAMPLE: using_global_request_and_reply
        internal async Task using_global_request_and_reply(IMessageContext messaging)
        {
            // Send a question to another application, and request that the handling
            // service send back an answer
            await messaging.SendAndExpectResponseFor<Answer>(new Question());
        }
        // ENDSAMPLE

        [Fact]
        public async Task invoke_expecting_a_response()
        {
            var answer = await Bus.Invoke<Answer>(new Question {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }


        [Fact]
        public async Task invoke_expecting_a_response_with_struct()
        {
            var answer = await Bus.Invoke<AnswerStruct>(new QuestionStruct {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }

        [Fact]
        public async Task invoke_with_expected_response_when_there_is_no_receiver()
        {
            await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Bus.Invoke<Answer>(new QuestionWithNoHandler());
            });
        }

        [Fact]
        public async Task invoke_with_no_known_response_do_not_blow_up()
        {
            (await Bus.Invoke<Answer>(new QuestionWithNoAnswer()))
                .ShouldBeNull();
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



    public class Question
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    public class Answer
    {
        public int Sum { get; set; }
        public int Product { get; set; }
    }

    public struct QuestionStruct
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    public struct AnswerStruct
    {
        public int Sum { get; set; }
        public int Product { get; set; }
    }

    public class QuestionWithNoHandler
    {
    }

    public class QuestionWithNoAnswer
    {
    }

    public class QuestionAndAnswerHandler
    {
        public Answer Handle(Question question)
        {
            return new Answer
            {
                Sum = question.One + question.Two,
                Product = question.One * question.Two
            };
        }

        public AnswerStruct Handle(QuestionStruct question)
        {
            return new AnswerStruct
            {
                Sum = question.One + question.Two,
                Product = question.One * question.Two
            };
        }

        public void Handle(QuestionWithNoAnswer question)
        {
        }
    }

}
