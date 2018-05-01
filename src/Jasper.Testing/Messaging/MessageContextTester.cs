using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class MessageContextTester
    {
        private readonly MockMessagingRoot theMessagingRoot = new MockMessagingRoot();
        private MessageContext theBus;

        public MessageContextTester()
        {
            var original = ObjectMother.Envelope();
            original.Id = Guid.NewGuid();
            original.OriginalId = Guid.NewGuid();

            theBus = theMessagingRoot.ContextFor(original).As<MessageContext>();
        }

        private void routedTo(Envelope envelope, params string[] destinations)
        {
            var outgoing = destinations.Select(x => new Envelope
            {
                Destination = x.ToUri(),
                Message = envelope?.Message ?? new Message1(),

            }).ToArray();

            var props = typeof(Envelope).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            var prop = props
                .FirstOrDefault(x => x.PropertyType == typeof(MessageRoute));

            foreach (var env in outgoing)
            {
                var route = new MessageRoute(typeof(Message1), env?.Destination ?? destinations.First().ToUri(), "application/json");
                route.Channel = Substitute.For<IChannel>();
                route.Channel.IsDurable.Returns(true);

                prop.SetValue(env, route);
            }

            if (envelope == null)
            {
                theMessagingRoot.Router.Route(Arg.Any<Envelope>()).Returns(outgoing);
            }
            else
            {
                theMessagingRoot.Router.Route(envelope).Returns(outgoing);
            }

        }

        [Fact]
        public async Task send_with_original_response()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Message = new Message1();

            routedTo(envelope, "tcp://server1:2222");

            await theBus.SendEnvelope(envelope);

            var outgoing = theBus.Outstanding.Single();

            outgoing.ParentId.ShouldBe(theBus.Envelope.Id);
            outgoing.OriginalId.ShouldBe(theBus.Envelope.OriginalId);
        }

        [Fact]
        public async Task adds_a_value_to_the_sent_time()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Message = new Message1();

            routedTo(envelope, "tcp://server1:2222");

            await theBus.SendEnvelope(envelope);

            var outgoing = theBus.Outstanding.Single();

            outgoing.SentTime.HasValue.ShouldBeTrue();
        }

        [Fact]
        public async Task publish_with_original_response()
        {
            routedTo(null, "tcp://server1:2222");
            await theBus.Publish(new Message1());

            var outgoing = theBus.Outstanding.Single();

            outgoing.ParentId.ShouldBe(theBus.Envelope.Id);
            outgoing.OriginalId.ShouldBe(theBus.Envelope.OriginalId);
        }




    }

    public class when_creating_a_service_bus_with_acknowledgement_required_envelope
    {
        private Envelope theEnvelope;
        private Envelope theAcknowledgement;

        public when_creating_a_service_bus_with_acknowledgement_required_envelope()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Id = Guid.NewGuid();
            theEnvelope.OriginalId = Guid.NewGuid();
            theEnvelope.ReplyUri = "tcp://server2:2000".ToUri();

            theEnvelope.AckRequested = true;

            var root = new MockMessagingRoot();
            var bus = root.ContextFor(theEnvelope);

            theAcknowledgement = bus.As<MessageContext>().Outstanding.Single();

        }

        [Fact]
        public void should_be_an_acknowledgement()
        {
            theAcknowledgement.ShouldNotBeNull();
        }

        [Fact]
        public void ack_parent_id()
        {
            theAcknowledgement.ParentId.ShouldBe(theEnvelope.Id);
        }

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
    }
}
