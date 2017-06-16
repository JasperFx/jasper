using System;
using System.Linq;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.ErrorHandling;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class using_error_handling_attributes : IntegrationContext
    {
        [Fact]
        public void use_maximum_attempts()
        {
            withAllDefaults();
            chainFor<Message1>().MaximumAttempts.ShouldBe(3);
        }

        [Fact]
        public void use_retry_on_attribute()
        {
            withAllDefaults();
            chainFor<Message2>().ShouldHandleExceptionWith<DivideByZeroException, RetryNowContinuation>();
        }

        [Fact]
        public void use_requeue_on_attribute()
        {
            withAllDefaults();
            chainFor<Message3>().ShouldHandleExceptionWith<NotImplementedException, RequeueContinuation>();
        }

        [Fact]
        public void use_move_to_error_queue_on_attribute()
        {
            withAllDefaults();

            chainFor<Message4>().ShouldMoveToErrorQueue<DataMisalignedException>();
        }

        [Fact]
        public void use_retry_later_attribute()
        {
            withAllDefaults();

            var continuation = chainFor<Message5>()
                .ErrorHandlers.OfType<ErrorHandler>()
                .Where(x => x.Conditions.Count == 1 &&
                            x.Conditions.Single() is ExceptionTypeMatch<DivideByZeroException>)
                .SelectMany(x => x.Sources)
                .OfType<ContinuationSource>().Select(x => x.Continuation)
                .OfType<DelayedRetryContinuation>()
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