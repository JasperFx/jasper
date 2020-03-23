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


            MessageSucceededContinuation.Instance
                .Execute(new MockMessagingRoot(), theChannel, theEnvelope, theOutgoingMessages, DateTime.UtcNow);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IChannelCallback theChannel = Substitute.For<IChannelCallback>();

        private readonly IQueuedOutgoingMessages theOutgoingMessages = Substitute.For<IQueuedOutgoingMessages>();

        [Fact]
        public void should_mark_the_message_as_successful()
        {
            theChannel.Received().Complete(theEnvelope);
        }

        [Fact]
        public void should_send_off_all_queued_up_cascaded_messages()
        {
            theOutgoingMessages.Received().SendAllQueuedOutgoingMessages();
        }
    }

    public class MessageSucceededContinuation_failure_handling_Tester
    {
        public MessageSucceededContinuation_failure_handling_Tester()
        {
            theOutgoingMessages.When(x => x.SendAllQueuedOutgoingMessages())
                .Throw(theException);


            MessageSucceededContinuation.Instance
                .Execute(theRoot, theChannel, theEnvelope, theOutgoingMessages, DateTime.UtcNow);
        }

        private readonly IMessagingRoot theRoot = new MockMessagingRoot();
        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IChannelCallback theChannel = Substitute.For<IChannelCallback>();
        private readonly Exception theException = new DivideByZeroException();
        private readonly IQueuedOutgoingMessages theOutgoingMessages = Substitute.For<IQueuedOutgoingMessages>();

        [Fact]
        public void should_send_a_failure_ack()
        {
            var message = "Sending cascading message failed: " + theException.Message;
            theRoot.Acknowledgements.Received().SendFailureAcknowledgement(theEnvelope, message);
        }
    }
}
