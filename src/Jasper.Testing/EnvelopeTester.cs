using System;
using Baseline.Dates;
using Jasper.Runtime.Scheduled;
using Jasper.Testing.Messaging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;
using NSubstitute;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing
{
    public class EnvelopeTester
    {
        [Fact]
        public void automatically_set_the_message_type_header_off_of_the_message()
        {
            var envelope = new Envelope
            {
                Message = new Message1(),
                Headers =
                {
                    ["a"] = "1",
                    ["b"] = "2"
                }
            };

            envelope.MessageType.ShouldBe(typeof(Message1).ToMessageTypeName());
        }

        [Fact]
        public void default_values_for_original_and_parent_id_are_null()
        {
            var parent = new Envelope();

            parent.CorrelationId.ShouldBeNull();
            parent.CausationId.ShouldBeNull();
        }

        [Fact]
        public void envelope_for_ping()
        {
            var envelope = Envelope.ForPing(TransportConstants.LocalUri);
            envelope.MessageType.ShouldBe(Envelope.PingMessageType);
            envelope.Data.ShouldNotBeNull();
        }


        [Fact]
        public void execution_time_is_null_by_default()
        {
            new Envelope().ExecutionTime.ShouldBeNull();
        }


        [Fact]
        public void for_response_copies_the_saga_id_from_the_parent()
        {
            var parent = ObjectMother.Envelope();
            parent.SagaId = Guid.NewGuid().ToString();

            var response = parent.CreateForResponse(new Message2());
            response.SagaId.ShouldBe(parent.SagaId);
        }


        [Fact]
        public void has_a_correlation_id_by_default()
        {
            new Envelope().Id.ShouldNotBe(Guid.Empty);

            new Envelope().Id.ShouldNotBe(new Envelope().Id);
            new Envelope().Id.ShouldNotBe(new Envelope().Id);
            new Envelope().Id.ShouldNotBe(new Envelope().Id);
            new Envelope().Id.ShouldNotBe(new Envelope().Id);
            new Envelope().Id.ShouldNotBe(new Envelope().Id);
        }

        [Fact]
        public void if_reply_requested_header_exists_in_parent_and_matches_the_message_type()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message1).ToMessageTypeName()
            };

            var childMessage = new Message1();

            var child = parent.CreateForResponse(childMessage);

            child.CausationId.ShouldBe(parent.CorrelationId);
            child.Destination.ShouldBe(parent.ReplyUri);
        }

        [Fact]
        public void is_expired()
        {
            var envelope = new Envelope
            {
                DeliverBy = null
            };

            envelope.IsExpired().ShouldBeFalse();

            envelope.DeliverBy = DateTime.UtcNow.AddSeconds(-1);
            envelope.IsExpired().ShouldBeTrue();

            envelope.DeliverBy = DateTime.UtcNow.AddHours(1);

            envelope.IsExpired().ShouldBeFalse();
        }

        [Fact]
        public void original_message_creating_child_envelope()
        {
            var parent = new Envelope();

            var childMessage = new Message1();

            var child = parent.CreateForResponse(childMessage);

            child.Message.ShouldBeSameAs(childMessage);

            child.CorrelationId.ShouldBe(parent.CorrelationId);
            child.CausationId.ShouldBe(parent.CorrelationId);
        }

        [Fact]
        public void parent_that_is_not_original_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            var childMessage = new Message1();

            var child = parent.CreateForResponse(childMessage);

            child.Message.ShouldBeSameAs(childMessage);

            child.CorrelationId.ShouldBe(parent.CorrelationId);
            child.CausationId.ShouldBe(parent.CorrelationId);
        }

        [Fact]
        public void set_deliver_by_threshold()
        {
            var envelope = new Envelope();

            envelope.DeliverWithin(5.Minutes());

            envelope.DeliverBy.ShouldNotBeNull();

            envelope.DeliverBy.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddMinutes(5).AddSeconds(-5));
            envelope.DeliverBy.Value.ShouldBeLessThan(DateTimeOffset.UtcNow.AddMinutes(5).AddSeconds(5));
        }

        [Fact]
        public void mark_received_when_not_delayed_execution()
        {
            var envelope = ObjectMother.Envelope();
            envelope.ExecutionTime = null;

            var uri = TransportConstants.LocalUri;
            var uniqueNodeId = 3;

            envelope.MarkReceived(uri, DateTime.UtcNow, uniqueNodeId);

            envelope.Status.ShouldBe(EnvelopeStatus.Incoming);
            envelope.OwnerId.ShouldBe(uniqueNodeId);

        }


        [Fact]
        public void mark_received_when_expired_execution()
        {
            var envelope = ObjectMother.Envelope();
            envelope.ExecutionTime = DateTime.UtcNow.AddDays(-1);

            var uri = TransportConstants.LocalUri;
            var uniqueNodeId = 3;

            envelope.MarkReceived(uri, DateTime.UtcNow, uniqueNodeId);

            envelope.Status.ShouldBe(EnvelopeStatus.Incoming);
            envelope.OwnerId.ShouldBe(uniqueNodeId);

        }

        [Fact]
        public void mark_received_when_it_has_a_later_execution_time()
        {
            var envelope = ObjectMother.Envelope();
            envelope.ExecutionTime = DateTime.UtcNow.AddDays(1);

            var uri = TransportConstants.LocalUri;
            var uniqueNodeId = 3;

            envelope.MarkReceived(uri, DateTime.UtcNow, uniqueNodeId);

            envelope.Status.ShouldBe(EnvelopeStatus.Scheduled);
            envelope.OwnerId.ShouldBe(TransportConstants.AnyNode);

        }


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
                theScheduledEnvelope.Status.ShouldBe(EnvelopeStatus.Scheduled);
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
}
