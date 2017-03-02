using System;
using JasperBus.ErrorHandling;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
{
    public class RetryNowContinuationTester
    {
        [Fact]
        public void just_calls_through_to_the_context_pipeline_to_do_it_again()
        {
            var continuation = RetryNowContinuation.Instance;

            var context = Substitute.For<IEnvelopeContext>();

            var theEnvelope = ObjectMother.Envelope();
            continuation.Execute(theEnvelope, context, DateTime.UtcNow);

            context.Received(1).Retry(theEnvelope);
        }
    }
}