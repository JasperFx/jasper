using System;
using JasperBus.ErrorHandling;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
{
    public class ErrorHandlerTester
    {
        [Fact]
        public void continuation_is_move_to_error_queue_by_default()
        {
            new ErrorHandler().Continuation(null, null)
                .ShouldBeOfType<RequeueContinuation>();
        }

        [Fact]
        public void matches_with_no_rules_is_true()
        {
            new ErrorHandler().Matches(ObjectMother.Envelope(), new Exception())
                .ShouldBeTrue();
        }

        [Fact]
        public void if_there_are_conditions_all_conditions_must_be_true_to_match()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var matchingCondition1 = Substitute.For<IExceptionMatch>();
            var matchingCondition2 = Substitute.For<IExceptionMatch>();
            var matchingCondition3 = Substitute.For<IExceptionMatch>();
            var conditionThatDoesNotMatch = Substitute.For<IExceptionMatch>();


            matchingCondition1.Matches(envelope, exception).Returns(true);
            matchingCondition2.Matches(envelope, exception).Returns(true);
            matchingCondition3.Matches(envelope, exception).Returns(true);

            var handler = new ErrorHandler();

            handler.AddCondition(matchingCondition1);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(matchingCondition2);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(matchingCondition3);
            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.AddCondition(conditionThatDoesNotMatch);
            handler.Matches(envelope, exception).ShouldBeFalse();
        }

        [Fact]
        public void if_nothing_matches_do_not_return_a_continuation()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var conditionThatDoesNotMatch = Substitute.For<IExceptionMatch>();


            var handler = new ErrorHandler();
            handler.AddCondition(conditionThatDoesNotMatch);

            handler.DetermineContinuation(envelope, exception)
                .ShouldBeNull();
        }

        [Fact]
        public void return_the_continuation_if_the_handler_matches()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var matchingCondition1 = Substitute.For<IExceptionMatch>();
            matchingCondition1.Matches(envelope, exception).Returns(true);

            var handler = new ErrorHandler();

            handler.AddCondition(matchingCondition1);
            handler.AddContinuation(Substitute.For<IContinuation>());

            handler.Matches(envelope, exception).ShouldBeTrue();

            handler.DetermineContinuation(envelope, exception)
                .ShouldBeTheSameAs(handler.Continuation(null, null));
        }
    }
}