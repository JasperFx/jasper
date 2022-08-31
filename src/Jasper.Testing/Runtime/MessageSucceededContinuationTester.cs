using System;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Transports;
using LamarCodeGeneration.Util;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class MessageSucceededContinuationTester
    {
        public MessageSucceededContinuationTester()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();

            theContext.Envelope.Returns(theEnvelope);

            MessageSucceededContinuation.Instance
                .ExecuteAsync(theContext, theRuntime, DateTimeOffset.Now);
        }

        private readonly MockJasperRuntime theRuntime = new MockJasperRuntime();
        private readonly Envelope theEnvelope = ObjectMother.Envelope();

        private readonly IMessageContext theContext = Substitute.For<IMessageContext>();

        [Fact]
        public void should_mark_the_message_as_successful()
        {
            theContext.Received().CompleteAsync();
        }

        [Fact]
        public void should_send_off_all_queued_up_cascaded_messages()
        {
            theContext.Received().FlushOutgoingMessagesAsync();
        }
    }

    public class MessageSucceededContinuation_failure_handling_Tester
    {
        public MessageSucceededContinuation_failure_handling_Tester()
        {
            theContext.When(x => x.FlushOutgoingMessagesAsync())
                .Throw(theException);

            theContext.Envelope.Returns(theEnvelope);

            MessageSucceededContinuation.Instance
                .ExecuteAsync(theContext, theRuntime, DateTimeOffset.Now);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly Exception theException = new DivideByZeroException();
        private readonly IMessageContext theContext = Substitute.For<IMessageContext>();
        private readonly MockJasperRuntime theRuntime = new MockJasperRuntime();

        [Fact]
        public void should_send_a_failure_ack()
        {
            var message = "Sending cascading message failed: " + theException.Message;
            theContext.Received().SendFailureAcknowledgementAsync(message);
        }
    }
}
