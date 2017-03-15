using System;
using System.Threading.Tasks;
using JasperBus.ErrorHandling;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
{
    public class RespondWithMessageContinuationTester
    {
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly object theMessage = new object();
        private readonly IEnvelopeContext theContext = Substitute.For<IEnvelopeContext>();

        [Fact]
        public async Task should_send_the_message()
        {
            await new RespondWithMessageContinuation(theMessage).Execute(theEnvelope, theContext, DateTime.Now);
            theContext.Received().SendOutgoingMessage(theEnvelope, theMessage);
        }
    }
}