using System;
using System.Threading.Tasks;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class invoke_message_and_expect_a_response : SendingContext
    {
        // SAMPLE: using_global_request_and_reply
        internal async Task using_global_request_and_reply(IMessageContext messaging)
        {
            // Send a question to another application, and request that the handling
            // service send back an answer
            await messaging.SendAndExpectResponseFor<Answer>(new Question());
        }

        [Fact]
        public async Task invoke_expecting_a_response()
        {
            StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });

            var answer = await theReceiver.Get<IMessagePublisher>().Invoke<Answer>(new Question {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }


        [Fact]
        public async Task invoke_expecting_a_response_with_struct()
        {
            StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });

            var answer = await theReceiver.Get<IMessagePublisher>().Invoke<AnswerStruct>(new QuestionStruct {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }

        [Fact]
        public async Task invoke_with_expected_response_when_there_is_no_receiver()
        {
            StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });
            await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            {
                await theReceiver.Get<IMessagePublisher>().Invoke<Answer>(new QuestionWithNoHandler());
            });
        }

        [Fact]
        public async Task invoke_with_no_known_response_do_not_blow_up()
        {
            StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });
            (await theReceiver.Get<IMessagePublisher>().Invoke<Answer>(new QuestionWithNoAnswer()))
                .ShouldBeNull();
        }

        // ENDSAMPLE
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

    public class QuestionAndAnswer
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
