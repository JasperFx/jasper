using System;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Runtime;
using NSubstitute;
using Xunit;

namespace MessagingTests.ErrorHandling
{
    public class RespondWithMessageContinuationTester
    {
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly object theMessage = new object();
        private readonly IMessageContext theContext = Substitute.For<IMessageContext>();

        [Fact]
        public async Task should_send_the_message()
        {
            var advanced = Substitute.For<IAdvancedMessagingActions>();
            theContext.Advanced.Returns(advanced);

            await new RespondWithMessageContinuation(theMessage).Execute(theContext, DateTime.Now);
            await advanced.Received().EnqueueCascading(theMessage);
        }
    }
}
