using System;
using JasperBus.ErrorHandling;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
{
    public class RespondWithMessageContinuationTester
    {
        public RespondWithMessageContinuationTester()
        {
            new RespondWithMessageContinuation(theMessage).Execute(theEnvelope, theContext, DateTime.Now);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly object theMessage = new object();
        private readonly IEnvelopeContext theContext = Substitute.For<IEnvelopeContext>();

        [Fact]
        public void should_send_the_message()
        {
            theContext.Received().SendOutgoingMessage(theEnvelope, theMessage);
        }
    }
}