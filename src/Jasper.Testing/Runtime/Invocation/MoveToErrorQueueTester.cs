using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Testing.Messaging;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Runtime.Invocation
{
    public class MoveToErrorQueueTester
    {
        public MoveToErrorQueueTester()
        {
            theContinuation = new MoveToErrorQueue(theException);

            theContext.Envelope.Returns(theEnvelope);

            advanced = Substitute.For<IAdvancedMessagingActions>();
            theContext.Advanced.Returns(advanced);
        }

        private readonly Exception theException = new DivideByZeroException();
        private readonly MoveToErrorQueue theContinuation;
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IMessageContext theContext = Substitute.For<IMessageContext>();
        private readonly IAdvancedMessagingActions advanced;

        [Fact]
        public async Task should_mark_the_envelope_as_failed()
        {
            await theContinuation.Execute(theContext, DateTime.UtcNow);

            await theEnvelope.Callback.Received().MoveToErrors(theEnvelope, theException);
        }

        [Fact]
        public async Task should_send_a_failure_ack()
        {
            await theContinuation.Execute(theContext, DateTime.UtcNow);

            await advanced.Received()
                .SendFailureAcknowledgement($"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}");
        }
    }
}
