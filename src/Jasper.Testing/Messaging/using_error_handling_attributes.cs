using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.ErrorHandling;
using Jasper.Testing.Messaging.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class using_error_handling_attributes : IntegrationContext
    {
        [Fact]
        public async Task use_maximum_attempts()
        {
            await withAllDefaults();
            chainFor<Message1>().MaximumAttempts.ShouldBe(3);
        }

        [Fact]
        public async Task use_retry_on_attribute()
        {
            await withAllDefaults();
            chainFor<Message2>().ShouldHandleExceptionWith<DivideByZeroException, RetryNowContinuation>();
        }

        [Fact]
        public async Task use_requeue_on_attribute()
        {
            await withAllDefaults();
            chainFor<Message3>().ShouldHandleExceptionWith<NotImplementedException, RequeueContinuation>();
        }

        [Fact]
        public async Task use_move_to_error_queue_on_attribute()
        {
            await withAllDefaults();

            chainFor<Message4>().ShouldMoveToErrorQueue<DataMisalignedException>();
        }

        [Fact]
        public async Task use_retry_later_attribute()
        {
            await withAllDefaults();

            var continuation = chainFor<Message5>()
                .ErrorHandlers.OfType<ErrorHandler>()
                .Where(x => x.Conditions.Count == 1 &&
                            x.Conditions.Single() is ExceptionTypeMatch<DivideByZeroException>)
                .SelectMany(x => x.Sources)
                .OfType<ContinuationSource>().Select(x => x.Continuation)
                .OfType<ScheduledRetryContinuation>()
                .Single();

            continuation.Delay.ShouldBe(5.Seconds());
        }
    }

    public class ErrorCausingConsumer
    {
        [MaximumAttempts(3)]
        public void Handle(Message1 message)
        {

        }

        [RetryOn(typeof(DivideByZeroException))]
        public void Handle(Message2 message)
        {

        }

        [RequeueOn(typeof(NotImplementedException))]
        public void Handle(Message3 message)
        {

        }

        [MoveToErrorQueueOn(typeof(DataMisalignedException))]
        public void Handle(Message4 message)
        {

        }

        [RetryLaterOn(typeof(DivideByZeroException), 5)]
        public void Handle(Message5 message)
        {

        }
    }
}
