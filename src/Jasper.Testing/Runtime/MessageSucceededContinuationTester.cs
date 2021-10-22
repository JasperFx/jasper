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

            theExecutionMessages.Envelope.Returns(theEnvelope);

            MessageSucceededContinuation.Instance
                .Execute(theChannel, theExecutionMessages, DateTime.UtcNow);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IChannelCallback theChannel = Substitute.For<IChannelCallback>();

        private readonly IExecutionContext theExecutionMessages = Substitute.For<IExecutionContext>();

        [Fact]
        public void should_mark_the_message_as_successful()
        {
            theChannel.Received().Complete(theEnvelope);
        }

        [Fact]
        public void should_send_off_all_queued_up_cascaded_messages()
        {
            theExecutionMessages.Received().SendAllQueuedOutgoingMessages();
        }
    }

    public class MessageSucceededContinuation_failure_handling_Tester
    {
        public MessageSucceededContinuation_failure_handling_Tester()
        {
            theContext.When(x => x.SendAllQueuedOutgoingMessages())
                .Throw(theException);

            theContext.Envelope.Returns(theEnvelope);

            MessageSucceededContinuation.Instance
                .Execute(theChannel, theContext, DateTime.UtcNow);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IChannelCallback theChannel = Substitute.For<IChannelCallback>();
        private readonly Exception theException = new DivideByZeroException();
        private readonly IExecutionContext theContext = Substitute.For<IExecutionContext>();

        [Fact]
        public void should_send_a_failure_ack()
        {
            var message = "Sending cascading message failed: " + theException.Message;
            theContext.Received().SendFailureAcknowledgement(theEnvelope, message);
        }
    }
}
