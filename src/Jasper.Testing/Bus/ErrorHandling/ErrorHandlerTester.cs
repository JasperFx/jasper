using System;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime.Invocation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.ErrorHandling
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
            ShouldBeBooleanExtensions.ShouldBeTrue(new ErrorHandler().Matches(ObjectMother.Envelope(), new Exception()));
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
            ShouldBeBooleanExtensions.ShouldBeTrue(handler.Matches(envelope, exception));

            handler.AddCondition(matchingCondition2);
            ShouldBeBooleanExtensions.ShouldBeTrue(handler.Matches(envelope, exception));

            handler.AddCondition(matchingCondition3);
            ShouldBeBooleanExtensions.ShouldBeTrue(handler.Matches(envelope, exception));

            handler.AddCondition(conditionThatDoesNotMatch);
            ShouldBeBooleanExtensions.ShouldBeFalse(handler.Matches(envelope, exception));
        }

        [Fact]
        public void if_nothing_matches_do_not_return_a_continuation()
        {
            var exception = new Exception();
            var envelope = ObjectMother.Envelope();

            var conditionThatDoesNotMatch = Substitute.For<IExceptionMatch>();


            var handler = new ErrorHandler();
            handler.AddCondition(conditionThatDoesNotMatch);

            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(envelope, exception));
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

            ShouldBeBooleanExtensions.ShouldBeTrue(handler.Matches(envelope, exception));

            handler.DetermineContinuation(envelope, exception)
                .ShouldBeTheSameAs(handler.Continuation(null, null));
        }
    }
}