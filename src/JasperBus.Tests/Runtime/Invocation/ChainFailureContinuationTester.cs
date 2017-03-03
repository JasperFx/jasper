using System;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace JasperBus.Tests.Runtime.Invocation
{
    public class ChainFailureContinuationTester
    {
        private Exception theException = new DivideByZeroException();
        private ChainFailureContinuation theContinuation;
        private Envelope theEnvelope = ObjectMother.Envelope();
        private IEnvelopeContext theContext = Substitute.For<IEnvelopeContext>();

        public ChainFailureContinuationTester()
        {
            theContinuation = new ChainFailureContinuation(theException);

            theContinuation.Execute(theEnvelope, theContext, DateTime.UtcNow);
        }

        [Fact]
        public void should_mark_the_envelope_as_failed()
        {
            // TODO -- should this be going to the error or dead letter queue instead?
            theEnvelope.Callback.Received().MarkFailed(theException);
        }

        [Fact]
        public void should_log_the_actual_exception()
        {
            theContext.ReceivedWithAnyArgs().Error(theEnvelope.CorrelationId, "", theException);
        }

        [Fact]
        public void should_send_a_failure_ack()
        {
            theContext.Received().SendFailureAcknowledgement(theEnvelope, "Message handler failed");
        }
    }
}