using System;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Runtime
{
    public class EnvelopeTester
    {
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
        public void envelope_for_ping()
        {
            var envelope = Envelope.ForPing();
            envelope.MessageType.ShouldBe(TransportConstants.PingMessageType);
            envelope.Data.ShouldNotBeNull();
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
        public void default_values_for_original_and_parent_id_are_null()
        {
            var parent = new Envelope
            {

            };

            parent.OriginalId.ShouldBe(Guid.Empty);
            parent.ParentId.ShouldBe(Guid.Empty);
        }

        [Fact]
        public void original_message_creating_child_envelope()
        {
            var parent = new Envelope
            {
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldBe(parent.Id);
            child.ParentId.ShouldBe(parent.Id);
        }


        [Fact]
        public void for_response_copies_the_saga_id_from_the_parent()
        {
            var parent = ObjectMother.Envelope();
            parent.SagaId = Guid.NewGuid().ToString();

            var response = parent.ForResponse(new Message2());
            response.SagaId.ShouldBe(parent.SagaId);
        }

        [Fact]
        public void parent_that_is_not_original_creating_child_envelope()
        {
            var parent = new Envelope
            {
                OriginalId = Guid.NewGuid()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldBe(parent.OriginalId);
            child.ParentId.ShouldBe(parent.Id);
        }

        [Fact]
        public void if_reply_requested_header_exists_in_parent_and_matches_the_message_type()
        {
            var parent = new Envelope
            {
                OriginalId = Guid.NewGuid(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message1).ToMessageAlias()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.ResponseId.ShouldBe(parent.Id);
            child.Destination.ShouldBe(parent.ReplyUri);
        }


        [Fact]
        public void if_reply_requested_header_exists_in_parent_and_does_NOT_match_the_message_type()
        {
            var parent = new Envelope
            {
                OriginalId = Guid.NewGuid(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message2).Name
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.ResponseId.ShouldBe(Guid.Empty);
            child.Destination.ShouldBeNull();
        }

        [Fact]
        public void do_not_set_destination_or_response_if_requested_header_does_not_exist_in_parent()
        {
            var parent = new Envelope
            {
                Id = Guid.NewGuid(),
                OriginalId = Guid.NewGuid(),
                Source = "foo://bar"
            };


            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);
            child.ResponseId.ShouldBe(Guid.Empty);
            child.Destination.ShouldBeNull();
        }


        [Fact]
        public void execution_time_is_null_by_default()
        {
            new Envelope().ExecutionTime.ShouldBeNull();
        }



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

            envelope.MessageType.ShouldBe(typeof(Message1).ToMessageAlias());
        }
    }

    [MessageAlias("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageAlias("Message2")]
    public class Message2
    {
        public Guid Id = Guid.NewGuid();
    }

    [MessageAlias("Message3")]
    public class Message3
    {

    }

    [MessageAlias("Message4")]
    public class Message4
    {

    }

    [MessageAlias("Message5")]
    public class Message5
    {
        public Guid Id = Guid.NewGuid();

        public int FailThisManyTimes = 0;
    }
}
