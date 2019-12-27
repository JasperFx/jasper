using System;
using Jasper.Runtime;
using Jasper.Runtime.Invocation;
using Jasper.Testing.Messaging;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Runtime.Invocation
{
    public class MessageSucceededContinuationTester
    {
        public MessageSucceededContinuationTester()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();

            theMessageContext.Envelope.Returns(theEnvelope);

            MessageSucceededContinuation.Instance
                .Execute(theMessageContext, DateTime.UtcNow);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IMessageContext theMessageContext = Substitute.For<IMessageContext>();

        [Fact]
        public void should_mark_the_message_as_successful()
        {
            theEnvelope.Callback.Received().MarkComplete();
        }

        [Fact]
        public void should_send_off_all_queued_up_cascaded_messages()
        {
            theMessageContext.Received().SendAllQueuedOutgoingMessages();
        }
    }

    public class MessageSucceededContinuation_failure_handling_Tester
    {
        public MessageSucceededContinuation_failure_handling_Tester()
        {
            theMessageContext.When(x => x.SendAllQueuedOutgoingMessages())
                .Throw(theException);

            theMessageContext.Envelope.Returns(theEnvelope);

            advanced = Substitute.For<IAdvancedMessagingActions>();
            theMessageContext.Advanced.Returns(advanced);

            MessageSucceededContinuation.Instance
                .Execute(theMessageContext, DateTime.UtcNow);
        }

        private readonly Envelope theEnvelope = ObjectMother.Envelope();
        private readonly IMessageContext theMessageContext = Substitute.For<IMessageContext>();
        private readonly Exception theException = new DivideByZeroException();
        private readonly IAdvancedMessagingActions advanced;

        [Fact]
        public void should_log_the_exception()
        {
            theEnvelope.Callback.Received().MoveToErrors(theEnvelope, theException);
        }


        [Fact]
        public void should_move_the_envelope_to_the_error_queue()
        {
            theEnvelope.Callback.Received().MoveToErrors(theEnvelope, theException);
        }

        [Fact]
        public void should_send_a_failure_ack()
        {
            var message = "Sending cascading message failed: " + theException.Message;
            advanced.Received().SendFailureAcknowledgement(message);
        }
    }
}
