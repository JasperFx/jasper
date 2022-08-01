using System;
using Jasper.ErrorHandling;
using Jasper.ErrorHandling.New;
using Jasper.Testing.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class FailureRuleTests
    {
        private readonly FailureRule theRule;
        private readonly Envelope theEnvelope = ObjectMother.Envelope();

        public FailureRuleTests()
        {
            theRule = new FailureRule(new TypeMatch<DivideByZeroException>());
        }

        [Fact]
        public void does_not_match_exception()
        {
            theRule.TryCreateContinuation(new BadImageFormatException(), theEnvelope, out var continuation)
                .ShouldBeFalse();
        }

        [Fact]
        public void matches_zero_attempt()
        {
            theRule.AddSlot(RequeueContinuation.Instance);

            // Should treat it as the 1st attempt
            theEnvelope.Attempts = 0;

            theRule.TryCreateContinuation(new DivideByZeroException(), theEnvelope, out var continuation)
                .ShouldBeTrue();

            continuation.ShouldBe(RequeueContinuation.Instance);
        }

        [Fact]
        public void matches_first_attempt()
        {
            theRule.AddSlot(RequeueContinuation.Instance);

            theEnvelope.Attempts = 1;

            theRule.TryCreateContinuation(new DivideByZeroException(), theEnvelope, out var continuation)
                .ShouldBeTrue();

            continuation.ShouldBe(RequeueContinuation.Instance);
        }

        [Fact]
        public void matches_second_attempt()
        {
            theRule.AddSlot(RequeueContinuation.Instance);
            theRule.AddSlot(RetryInlineContinuation.Instance);

            theEnvelope.Attempts = 2;

            theRule.TryCreateContinuation(new DivideByZeroException(), theEnvelope, out var continuation)
                .ShouldBeTrue();

            continuation.ShouldBe(RetryInlineContinuation.Instance);
        }

        [Fact]
        public void exceeds_known_slots_and_should_be_dead_letter_queued()
        {
            theRule.AddSlot(RequeueContinuation.Instance);
            theRule.AddSlot(RetryInlineContinuation.Instance);

            // This exceeds the known usages
            theEnvelope.Attempts = 3;

            theRule.TryCreateContinuation(new DivideByZeroException(), theEnvelope, out var continuation)
                .ShouldBeTrue();

            continuation.ShouldBeOfType<MoveToErrorQueue>();
        }


    }
}
