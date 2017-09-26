using System;
using System.Linq;
using System.Threading.Tasks;
using BlueMilk.Scanning;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.Bus.Runtime.Invocation;
using Jasper.Util;
using NSubstitute.Extensions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{


    public class when_requesting_a_response : InteractionContext<ServiceBus>
    {
        private RecordingEnvelopeSender theSender = new RecordingEnvelopeSender();
        private Message1 theRequest = new Message1();
        private Task<Message2> original = Task.FromResult(new Message2());
        private Task<Message2> theTask;
        private Envelope theEnvelope;

        public when_requesting_a_response()
        {
            Services.Inject<IEnvelopeSender>(theSender);

            MockFor<IReplyWatcher>()
                .ReturnsForAll(original);

            theTask = ClassUnderTest.Request<Message2>(theRequest);

            theEnvelope = theSender.Sent.Single();
        }

        [Fact]
        public void the_envelope_is_sent_with_reply_requested_header()
        {
            theEnvelope.ReplyRequested.ShouldBe(typeof(Message2).ToMessageAlias());
        }

        [Fact]
        public void sends_the_envelope_to_the_sender()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(theEnvelope);
        }

    }

    public class when_requesting_a_response_from_a_particular_destination     : InteractionContext<ServiceBus>
    {
        private RecordingEnvelopeSender theSender = new RecordingEnvelopeSender();
        private Message1 theRequest = new Message1();
        private Task<Message2> original = Task.FromResult(new Message2());
        private Task<Message2> theTask;
        private Envelope theEnvelope;
        private Uri destination = "stub://one".ToUri();

        public when_requesting_a_response_from_a_particular_destination()
        {
            Services.Inject<IEnvelopeSender>(theSender);

            MockFor<IReplyWatcher>()
                .ReturnsForAll(original);

            theTask = ClassUnderTest.Request<Message2>(theRequest, new RequestOptions{Destination = destination});

            theEnvelope = theSender.Sent.Single();
        }

        [Fact]
        public void the_envelope_should_have_the_designated_destination()
        {
            theEnvelope.Destination.ShouldBe(destination);
        }

        [Fact]
        public void the_envelope_is_sent_with_reply_requested_header()
        {
            theEnvelope.ReplyRequested.ShouldBe(typeof(Message2).ToMessageAlias());
        }

        [Fact]
        public void sends_the_envelope_to_the_sender()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(theEnvelope);
        }

    }
}
