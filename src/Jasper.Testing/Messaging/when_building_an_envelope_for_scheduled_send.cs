using System;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class when_building_an_envelope_for_scheduled_send
    {
        private Envelope theOriginal;
        private Envelope theScheduledEnvelope;
        private ISendingAgent theSubscriber;

        public when_building_an_envelope_for_scheduled_send()
        {
            theOriginal = ObjectMother.Envelope();
            theOriginal.ExecutionTime = DateTime.UtcNow.Date.AddDays(2);
            theOriginal.Destination = "tcp://server3:2345".ToUri();

            theSubscriber = Substitute.For<ISendingAgent>();

            theScheduledEnvelope = theOriginal.ForScheduledSend(theSubscriber);
        }

        [Fact]
        public void the_message_should_be_the_original_envelope()
        {
            theScheduledEnvelope.Message.ShouldBeSameAs(theOriginal);
        }

        [Fact]
        public void execution_time_is_copied()
        {
            theScheduledEnvelope.ExecutionTime.ShouldBe(theOriginal.ExecutionTime);
        }

        [Fact]
        public void destination_is_scheduled_queue()
        {
            theScheduledEnvelope.Destination.ShouldBe(TransportConstants.DurableLocalUri);
        }

        [Fact]
        public void status_should_be_scheduled()
        {
            theScheduledEnvelope.Status.ShouldBe(TransportConstants.Scheduled);
        }

        [Fact]
        public void owner_should_be_any_node()
        {
            theScheduledEnvelope.OwnerId.ShouldBe(TransportConstants.AnyNode);
        }

        [Fact]
        public void the_message_type_is_envelope()
        {
            theScheduledEnvelope.MessageType.ShouldBe(TransportConstants.ScheduledEnvelope);
        }

        [Fact]
        public void the_content_type_should_be_binary_envelope()
        {
            theScheduledEnvelope.ContentType.ShouldBe(TransportConstants.SerializedEnvelope);
        }

    }
}
