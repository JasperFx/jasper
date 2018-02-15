using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime.Invocation
{
    public class MoveToErrorQueueTester
    {
        private Exception theException = new DivideByZeroException();
        private MoveToErrorQueue theContinuation;
        private Envelope theEnvelope = ObjectMother.Envelope();
        private IMessageContext theContext = Substitute.For<IMessageContext>();
        private IAdvancedMessagingActions advanced;

        public MoveToErrorQueueTester()
        {
            theContinuation = new MoveToErrorQueue(theException);

            theContext.Envelope.Returns(theEnvelope);

            advanced = Substitute.For<IAdvancedMessagingActions>();
            theContext.Advanced.Returns(advanced);
        }

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

            await advanced.Received().SendFailureAcknowledgement($"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}");
        }
    }
}
