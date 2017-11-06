using System;
using System.Threading.Tasks;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Invocation
{
    public class MoveToErrorQueueTester
    {
        private Exception theException = new DivideByZeroException();
        private MoveToErrorQueue theContinuation;
        private Envelope theEnvelope = ObjectMother.Envelope();
        private IEnvelopeContext theContext = Substitute.For<IEnvelopeContext>();

        public MoveToErrorQueueTester()
        {
            theContinuation = new MoveToErrorQueue(theException);


        }

        [Fact]
        public async Task should_mark_the_envelope_as_failed()
        {
            await theContinuation.Execute(theEnvelope, theContext, DateTime.UtcNow);

            await theEnvelope.Callback.Received().MoveToErrors(new ErrorReport(theEnvelope, theException));
        }

        [Fact]
        public async Task should_send_a_failure_ack()
        {
            await theContinuation.Execute(theEnvelope, theContext, DateTime.UtcNow);

            await theContext.Received().SendFailureAcknowledgement(theEnvelope, $"Moved message {theEnvelope.Id} to the Error Queue.\n{theException}");
        }
    }
}
