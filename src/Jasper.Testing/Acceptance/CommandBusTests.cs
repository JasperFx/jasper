using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class CommandBusTests : IntegrationContext, ILogger<JasperRuntime>
    {
        private readonly WorkTracker theTracker = new WorkTracker();


        public CommandBusTests(DefaultApp @default) : base(@default)
        {
        }

        private void configure()
        {
            with(opts =>
            {
                opts.Services.AddSingleton(theTracker);

                opts.Publish(x => x.MessagesFromAssemblyContaining<Message1>()
                    .ToLocalQueue("cascading"));

                opts.Services.AddSingleton<ILogger<JasperRuntime>>(this);

                opts.Handlers.OnException<DivideByZeroException>().Requeue();

                opts.Handlers.Retries.MaximumAttempts = 3;
            });
        }



        public readonly IList<Exception> Exceptions = new List<Exception>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (exception != null) Exceptions.Add(exception);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return Substitute.For<IDisposable>();
        }


        [Fact]
        public async Task enqueue_locally()
        {
            var message = new Message1
            {
                Id = Guid.NewGuid()
            };

            var session = await Host.ExecuteAndWaitAsync(c => c.EnqueueAsync(message));

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


            await Should.ThrowAsync<DivideByZeroException>(() => Publisher.InvokeAsync(message));
        }

        [Fact]
        public async Task will_log_an_exception()
        {
            configure();

            try
            {
                await Publisher.InvokeAsync(new Message5 {FailThisManyTimes = 1});
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

            await Publisher.InvokeAsync(message);

            theTracker.LastMessage.ShouldBeSameAs(message);
        }

        [Fact]
        public async Task will_send_cascading_messages()
        {
            configure();

            var message = new Message5();

            await Publisher.InvokeAsync(message);

            var m1 = await theTracker.Message1;
            m1.Id.ShouldBe(message.Id);

            var m2 = await theTracker.Message2;
            m2.Id.ShouldBe(message.Id);
        }



        #region sample_using_global_request_and_reply
        internal async Task using_global_request_and_reply(IExecutionContext messaging)
        {
            // Send a question to another application, and request that the handling
            // service send back an answer
            await messaging.SendAndExpectResponseForAsync<Answer>(new Question());
        }
        #endregion

        [Fact]
        public async Task invoke_expecting_a_response()
        {
            var answer = await Bus.InvokeAsync<Answer>(new Question {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }


        [Fact]
        public async Task invoke_expecting_a_response_with_struct()
        {
            var answer = await Bus.InvokeAsync<AnswerStruct>(new QuestionStruct {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }

        [Fact]
        public async Task invoke_with_expected_response_when_there_is_no_receiver()
        {
            await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Bus.InvokeAsync<Answer>(new QuestionWithNoHandler());
            });
        }

        [Fact]
        public async Task invoke_with_no_known_response_do_not_blow_up()
        {
            (await Bus.InvokeAsync<Answer>(new QuestionWithNoAnswer()))
                .ShouldBeNull();
        }

        [Fact]
        public async Task should_return_result_for_command_with_castable_result()
        {
            var answer = await Bus.InvokeAsync<IAnswer>(new Question { One = 3, Two = 4 });

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
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

    public interface IAnswer
    {
        int Sum { get; }
        int Product { get; }
    }

    public class Answer : IAnswer
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
