using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class MessageContextTester
    {
        public MessageContextTester()
        {
            var original = ObjectMother.Envelope();
            original.Id = Guid.NewGuid();
            original.CorrelationId = Guid.NewGuid();

            theContext = theMessagingRoot.ContextFor(original).As<MessageContext>();
        }

        private readonly MockMessagingRoot theMessagingRoot = new MockMessagingRoot();
        private readonly MessageContext theContext;

        private void routedTo(Envelope envelope, params string[] destinations)
        {
            var outgoing = destinations.Select(x => new Envelope
            {
                Destination = x.ToUri(),
                Message = envelope?.Message ?? new Message1()
            }).ToArray();


            foreach (var env in outgoing)
            {
                var sender = Substitute.For<ISendingAgent>();
                sender.IsDurable.Returns(true);

                var subscriber = Substitute.For<ISubscriber>();
                subscriber.ShouldSendMessage(null).ReturnsForAnyArgs(false);

                theMessagingRoot.Subscribers.Add(env.Destination, subscriber);

            }

            if (envelope == null)
                theMessagingRoot.Router.RouteOutgoingByEnvelope(Arg.Any<Envelope>()).Returns(outgoing);
            else
                theMessagingRoot.Router.RouteOutgoingByEnvelope(envelope).Returns(outgoing);
        }

        [Fact]
        public void correlation_id_should_be_same_as_original_envelope()
        {
           theContext.CorrelationId.ShouldBe(theContext.Envelope.CorrelationId);
        }

        [Fact]
        public void new_context_gets_a_non_empty_correlation_id()
        {
            theMessagingRoot.NewContext().CorrelationId.ShouldNotBe(Guid.Empty);
        }


        [Fact]
        public async Task publish_with_original_response()
        {
            routedTo(null, "tcp://server1:2222");
            await theContext.Publish(new Message1());

            var outgoing = theContext.Outstanding.Single();

            outgoing.CausationId.ShouldBe(theContext.Envelope.Id);
            outgoing.CorrelationId.ShouldBe(theContext.Envelope.CorrelationId);
        }

        [Fact]
        public async Task send_with_original_response()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Message = new Message1();

            routedTo(envelope, "tcp://server1:2222");

            await theContext.SendEnvelope(envelope);

            var outgoing = theContext.Outstanding.Single();

            outgoing.CausationId.ShouldBe(theContext.Envelope.Id);
            outgoing.CorrelationId.ShouldBe(theContext.Envelope.CorrelationId);
        }
    }

    public class when_creating_a_service_bus_with_acknowledgement_required_envelope
    {
        public when_creating_a_service_bus_with_acknowledgement_required_envelope()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Id = Guid.NewGuid();
            theEnvelope.CorrelationId = Guid.NewGuid();
            theEnvelope.ReplyUri = "tcp://server2:2000".ToUri();

            theEnvelope.AckRequested = true;

            var root = new MockMessagingRoot();
            var bus = root.ContextFor(theEnvelope);

            theAcknowledgement = bus.As<MessageContext>().Outstanding.Single();
        }

        private readonly Envelope theEnvelope;
        private readonly Envelope theAcknowledgement;

        [Fact]
        public void ack_destination()
        {
            theAcknowledgement.Destination.ShouldBe(theEnvelope.ReplyUri);
        }

        [Fact]
        public void ack_message()
        {
            theAcknowledgement.Message.ShouldBeOfType<Acknowledgement>()
                .CorrelationId.ShouldBe(theEnvelope.Id);
        }

        [Fact]
        public void ack_parent_id()
        {
            theAcknowledgement.CausationId.ShouldBe(theEnvelope.Id);
        }

        [Fact]
        public void should_be_an_acknowledgement()
        {
            theAcknowledgement.ShouldNotBeNull();
        }
    }
}
