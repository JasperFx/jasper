using System;
using Baseline.Dates;
using Jasper.ErrorHandling;
using Jasper.Testing.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class ContinuationFactoryTester
    {
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly Exception theException = new ArithmeticException();
        private readonly ContinuationFactory theFactory = new ContinuationFactory();

        public ContinuationFactoryTester()
        {
            theEnvelope.Attempts = 1;
        }

        [Fact]
        public void retry_now()
        {
            theFactory.RetryNow();

            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RetryNowContinuation.Instance);
        }


        [Fact]
        public void requeue()
        {
            theFactory.Requeue();

            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RequeueContinuation.Instance);
        }


        [Fact]
        public void schedule_retry()
        {
            theFactory.ScheduleRetry(3.Seconds());

            theFactory.Build(theEnvelope, theException)
                .ShouldBe(new ScheduledRetryContinuation(3.Seconds()));
        }


        [Fact]
        public void discard()
        {
            theFactory.Discard();

            theFactory.Build(theEnvelope, theException)
                .ShouldBe(DiscardExpiredEnvelope.Instance);
        }


        [Fact]
        public void move_to_dead_letter_queue()
        {
            theFactory.MoveToErrorQueue();

            theFactory.Build(theEnvelope, theException)
                .ShouldBe(new MoveToErrorQueue(theException));
        }

        [Fact]
        public void build_in_order()
        {
            theFactory.RetryNow();
            theFactory.Requeue();
            theFactory.ScheduleRetry(5.Seconds());

            theEnvelope.Attempts = 1;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RetryNowContinuation.Instance);

            theEnvelope.Attempts = 2;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RequeueContinuation.Instance);

            theEnvelope.Attempts = 3;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(new ScheduledRetryContinuation(5.Seconds()));

        }

        [Fact]
        public void return_move_to_error_queue_by_default()
        {
            theFactory.RetryNow();
            theFactory.Requeue();

            theEnvelope.Attempts = 1;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RetryNowContinuation.Instance);

            theEnvelope.Attempts = 2;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(RequeueContinuation.Instance);

            theEnvelope.Attempts = 3;
            theFactory.Build(theEnvelope, theException)
                .ShouldBe(new MoveToErrorQueue(theException));

        }
    }
}
