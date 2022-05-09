using System;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class when_building_an_acknowledgement
    {
        public when_building_an_acknowledgement()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Id = Guid.NewGuid();
            theEnvelope.CorrelationId = Guid.NewGuid().ToString();
            theEnvelope.ReplyUri = "tcp://server2:2000".ToUri();

            theEnvelope.AckRequested = true;

            theAcknowledgement = new AcknowledgementSender(null, new MockJasperRuntime()).BuildAcknowledgement(theEnvelope);
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
            theAcknowledgement.CausationId.ShouldBe(theEnvelope.Id.ToString());
        }

        [Fact]
        public void should_be_an_acknowledgement()
        {
            theAcknowledgement.ShouldNotBeNull();
        }
    }
}
