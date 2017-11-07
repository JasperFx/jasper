using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Invocation
{
    public class MessageSucceededContinuationTester
    {
        private Envelope theEnvelope = ObjectMother.Envelope();
        private IEnvelopeContext theEnvelopeContext = Substitute.For<IEnvelopeContext>();


        public MessageSucceededContinuationTester()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();

            MessageSucceededContinuation.Instance
                .Execute(theEnvelope, theEnvelopeContext, DateTime.UtcNow);
        }

        [Fact]
        public void should_mark_the_message_as_successful()
        {
            theEnvelope.Callback.Received().MarkComplete();
        }

        [Fact]
        public void should_send_off_all_queued_up_cascaded_messages()
        {
            theEnvelopeContext.Received().SendAllQueuedOutgoingMessages();
        }

    }

    public class MessageSucceededContinuation_failure_handling_Tester
    {
        private Envelope theEnvelope = ObjectMother.Envelope();
        private IEnvelopeContext theEnvelopeContext = Substitute.For<IEnvelopeContext>();
        private readonly Exception theException = new DivideByZeroException();

        public MessageSucceededContinuation_failure_handling_Tester()
        {
            theEnvelopeContext.When(x => x.SendAllQueuedOutgoingMessages())
                .Throw(theException);

            MessageSucceededContinuation.Instance
                .Execute(theEnvelope, theEnvelopeContext, DateTime.UtcNow);
        }


        [Fact]
        public void should_move_the_envelope_to_the_error_queue()
        {
            theEnvelope.Callback.Received().MoveToErrors(theEnvelope, theException);
        }

        [Fact]
        public void should_log_the_exception()
        {
            theEnvelope.Callback.Received().MoveToErrors(theEnvelope, theException);
        }

        [Fact]
        public void should_send_a_failure_ack()
        {
            var message = "Sending cascading message failed: " + theException.Message;
            theEnvelopeContext.Received().SendFailureAcknowledgement(theEnvelope, message);

        }
    }



}
