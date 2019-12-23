using System;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime
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

            parent.CorrelationId.ShouldBe(Guid.Empty);
            parent.CausationId.ShouldBe(Guid.Empty);
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
            new Envelope().Id.ShouldNotBeNull();

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
                CorrelationId = Guid.NewGuid(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message1).ToMessageTypeName()
            };

            var childMessage = new Message1();

            var child = parent.CreateForResponse(childMessage);

            child.CausationId.ShouldBe(parent.Id);
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

            child.CorrelationId.ShouldBe(parent.Id);
            child.CausationId.ShouldBe(parent.Id);
        }

        [Fact]
        public void parent_that_is_not_original_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid()
            };

            var childMessage = new Message1();

            var child = parent.CreateForResponse(childMessage);

            child.Message.ShouldBeSameAs(childMessage);

            child.CorrelationId.ShouldBe(parent.CorrelationId);
            child.CausationId.ShouldBe(parent.Id);
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

            envelope.ReceivedAt.ShouldBe(uri);
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

            envelope.ReceivedAt.ShouldBe(uri);
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

            envelope.ReceivedAt.ShouldBe(uri);
        }

    }
}
