using System;
using Jasper.Messaging.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class MoveToErrorQueueHandlerTester
    {
        [Fact]
        public void do_nothing_if_it_is_not_the_right_exception()
        {
            var handler = new MoveToErrorQueueHandler<NotImplementedException>();
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new Exception()));
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new DivideByZeroException()));
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new NotSupportedException()));
        }

        [Fact]
        public void moves_to_the_error_queue_if_the_exception_matches()
        {
            var handler = new MoveToErrorQueueHandler<NotImplementedException>();
            var ex = new NotImplementedException();

            handler.DetermineContinuation(null, ex).ShouldBeOfType<MoveToErrorQueue>()
                .Exception.ShouldBeTheSameAs(ex);
        }


    }
}