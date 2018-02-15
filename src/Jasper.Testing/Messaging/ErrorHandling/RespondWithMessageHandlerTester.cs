using System;
using Jasper.Messaging.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class RespondWithMessageHandlerTester
    {

        [Fact]
        public void returns_no_continuation_when_exception_does_not_match()
        {
            var handler = new RespondWithMessageHandler<NotImplementedException>(null);
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new Exception()));
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new DivideByZeroException()));
            ShouldBeNullExtensions.ShouldBeNull(handler.DetermineContinuation(null, new NullReferenceException()));
        }

        [Fact]
        public void responds_with_message_when_the_exception_matches()
        {
            var message = new object();
            var handler = new RespondWithMessageHandler<Exception>((ex, env) => message);
            handler.DetermineContinuation(null, new Exception())
                .ShouldBeOfType<RespondWithMessageContinuation>()
                .Message.ShouldBeTheSameAs(message);
        }
    }
}