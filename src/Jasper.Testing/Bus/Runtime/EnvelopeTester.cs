using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime
{
    public class EnvelopeTester
    {
        [Fact]
        public void has_a_correlation_id_by_default()
        {
            new Envelope().CorrelationId.ShouldNotBeNull();

            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
        }


        [Fact]
        public void default_values_for_original_and_parent_id_are_null()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            parent.OriginalId.ShouldBeNull();
            parent.ParentId.ShouldBeNull();
        }

        [Fact]
        public void original_message_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldBe(parent.CorrelationId);
            child.ParentId.ShouldBe(parent.CorrelationId);
        }

        [Fact]
        public void parent_that_is_not_original_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldBe(parent.OriginalId);
            child.ParentId.ShouldBe(parent.CorrelationId);
        }

        [Fact]
        public void if_reply_requested_header_exists_in_parent_and_matches_the_message_type()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message1).ToMessageAlias()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.ResponseId.ShouldBe(parent.CorrelationId);
            child.Destination.ShouldBe(parent.ReplyUri);
        }


        [Fact]
        public void if_reply_requested_header_exists_in_parent_and_does_NOT_match_the_message_type()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString(),
                ReplyUri = "foo://bar".ToUri(),
                ReplyRequested = typeof(Message2).Name
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.ResponseId.ShouldBeNull();
            child.Destination.ShouldBeNull();
        }

        [Fact]
        public void do_not_set_destination_or_response_if_requested_header_does_not_exist_in_parent()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString(),
                Source = "foo://bar".ToUri()
            };


            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);
            child.ResponseId.ShouldBeNull();
            child.Destination.ShouldBeNull();
        }


        [Fact]
        public void execution_time_is_null_by_default()
        {
            new Envelope().ExecutionTime.ShouldBeNull();
        }



        [Fact]
        public void cloning_an_envelope()
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

            var clone = envelope.Clone();

            clone.ShouldNotBeTheSameAs(envelope);
            clone.Message.ShouldBeTheSameAs(envelope.Message);

            clone.Headers["a"].ShouldBe("1");
            clone.Headers["b"].ShouldBe("2");
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
