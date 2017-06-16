using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime
{
    public class EnvelopeTester
    {
        [Fact]
        public void has_a_correlation_id_by_default()
        {
            ShouldBeNullExtensions.ShouldNotBeNull(new Envelope().CorrelationId);

            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotBe(new Envelope().CorrelationId);
        }

        [Fact]
        public void does_not_override_an_existing_correlation_id()
        {
            var headers = new Dictionary<string, string> {[Envelope.IdKey] = "FOO"};

            var envelope = new Envelope(headers);
            envelope.CorrelationId.ShouldBe("FOO");
        }

        [Fact]
        public void will_assign_a_new_correlation_id_if_none_in_headers()
        {
            ShouldBeBooleanExtensions.ShouldBeFalse(new Envelope().CorrelationId
                    .IsEmpty());
        }

        [Fact]
        public void default_values_for_original_and_parent_id_are_null()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            ShouldBeNullExtensions.ShouldBeNull(parent.OriginalId);
            ShouldBeNullExtensions.ShouldBeNull(parent.ParentId);
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
                ReplyRequested = typeof(Message1).Name
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Headers[Envelope.ResponseIdKey].ShouldBe(parent.CorrelationId);
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

            ShouldBeBooleanExtensions.ShouldBeFalse(child.Headers.ContainsKey(Envelope.ResponseIdKey));
            ShouldBeNullExtensions.ShouldBeNull(child.Destination);
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

            ShouldBeBooleanExtensions.ShouldBeFalse(parent.Headers.ContainsKey(Envelope.ReplyRequestedKey));

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            ShouldBeBooleanExtensions.ShouldBeFalse(child.Headers.ContainsKey(Envelope.ResponseIdKey));
            ShouldBeNullExtensions.ShouldBeNull(child.Destination);
        }

        [Fact]
        public void source_property()
        {
            var envelope = new Envelope();

            ShouldBeNullExtensions.ShouldBeNull(envelope.Source);

            var uri = "fake://thing".ToUri();
            envelope.Source = uri;

            envelope.Headers[Envelope.SourceKey].ShouldBe(uri.ToString());
            envelope.Source.ShouldBe(uri);
        }

        [Fact]
        public void reply_uri_property()
        {
            var envelope = new Envelope();

            ShouldBeNullExtensions.ShouldBeNull(envelope.ReplyUri);

            var uri = "fake://thing".ToUri();
            envelope.ReplyUri = uri;

            envelope.Headers[Envelope.ReplyUriKey].ShouldBe(uri.ToString());
            envelope.ReplyUri.ShouldBe(uri);
        }


        [Fact]
        public void content_type()
        {
            var envelope = new Envelope();
            envelope.ContentType.ShouldBe(null);

            envelope.ContentType = "text/xml";

            envelope.Headers[Envelope.ContentTypeKey].ShouldBe("text/xml");
            envelope.ContentType.ShouldBe("text/xml");
        }

        [Fact]
        public void acceptable_content_types()
        {
            var envelope = new Envelope();
            envelope.AcceptedContentTypes.ShouldBeEmpty();

            envelope.AcceptedContentTypes = new [] {"application/json","application/xml"};

            envelope.Headers[Envelope.AcceptedContentTypesKey].ShouldBe("application/json,application/xml");
            envelope.AcceptedContentTypes.ShouldHaveTheSameElementsAs("application/json","application/xml");
        }

        [Fact]
        public void original_id()
        {
            var envelope = new Envelope();
            ShouldBeNullExtensions.ShouldBeNull(envelope.OriginalId);

            var originalId = Guid.NewGuid().ToString();
            envelope.OriginalId = originalId;

            envelope.Headers[Envelope.OriginalIdKey].ShouldBe(originalId);
            envelope.OriginalId.ShouldBe(originalId);
        }

        [Fact]
        public void ParentId()
        {
            var envelope = new Envelope();
            ShouldBeNullExtensions.ShouldBeNull(envelope.ParentId);

            var parentId = Guid.NewGuid().ToString();
            envelope.ParentId = parentId;

            envelope.Headers[Envelope.ParentIdKey].ShouldBe(parentId);
            envelope.ParentId.ShouldBe(parentId);
        }

        [Fact]
        public void ResponseId()
        {
            var envelope = new Envelope();
            ShouldBeNullExtensions.ShouldBeNull(envelope.ResponseId);

            var responseId = Guid.NewGuid().ToString();
            envelope.ResponseId = responseId;

            envelope.Headers[Envelope.ResponseIdKey].ShouldBe(responseId);
            envelope.ResponseId.ShouldBe(responseId);
        }

        [Fact]
        public void destination_property()
        {
            var envelope = new Envelope();

            ShouldBeNullExtensions.ShouldBeNull(envelope.Destination);

            var uri = "fake://thing".ToUri();
            envelope.Destination = uri;

            envelope.Headers[Envelope.DestinationKey].ShouldBe(uri.ToString());
            envelope.Destination.ShouldBe(uri);
        }

        [Fact]
        public void received_at_property()
        {
            var envelope = new Envelope();

            ShouldBeNullExtensions.ShouldBeNull(envelope.ReceivedAt);

            var uri = "fake://thing".ToUri();
            envelope.ReceivedAt = uri;

            envelope.Headers[Envelope.ReceivedAtKey].ShouldBe(uri.ToString());
            envelope.ReceivedAt.ShouldBe(uri);
        }

        [Fact]
        public void reply_requested()
        {
            var envelope = new Envelope();
            ShouldBeNullExtensions.ShouldBeNull(envelope.ReplyRequested);


            envelope.ReplyRequested = "Foo";
            envelope.Headers[Envelope.ReplyRequestedKey].ShouldBe("Foo");
            envelope.ReplyRequested.ShouldBe("Foo");

            envelope.ReplyRequested = null;
            ShouldBeNullExtensions.ShouldBeNull(envelope.ReplyRequested);
        }

        [Fact]
        public void ack_requested()
        {
            var envelope = new Envelope();
            ShouldBeBooleanExtensions.ShouldBeFalse(envelope.AckRequested);


            envelope.AckRequested = true;
            envelope.Headers[Envelope.AckRequestedKey].ShouldBe("true");
            ShouldBeBooleanExtensions.ShouldBeTrue(envelope.AckRequested);

            envelope.AckRequested = false;
            ShouldBeBooleanExtensions.ShouldBeFalse(envelope.Headers.ContainsKey(Envelope.AckRequestedKey));
        }

        [Fact]
        public void execution_time_is_null_by_default()
        {
            ShouldBeNullExtensions.ShouldBeNull(new Envelope().ExecutionTime);
        }

        [Fact]
        public void execution_time_set_and_get()
        {
            var time = DateTime.Today.AddHours(8).ToUniversalTime();

            var envelope = new Envelope();
            envelope.ExecutionTime = time;

            envelope.ExecutionTime.ShouldBe(time);
        }

        [Fact]
        public void nulling_out_the_execution_time()
        {
            var time = DateTime.Today.AddHours(8).ToUniversalTime();

            var envelope = new Envelope {ExecutionTime = time};

            envelope.ExecutionTime = null;

            ShouldBeNullExtensions.ShouldBeNull(envelope.ExecutionTime);
        }

        [Fact]
        public void attempts()
        {
            var envelope = new Envelope();
            envelope.Attempts.ShouldBe(0);

            envelope.Attempts++;

            envelope.Attempts.ShouldBe(1);
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
            clone.Headers.ShouldNotBeTheSameAs(envelope.Headers);

            clone.Headers["a"].ShouldBe("1");
            clone.Headers["b"].ShouldBe("2");
        }
    }

    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }

    public class Message2
    {
        
    }

    public class Message3
    {

    }

    public class Message4
    {

    }

    public class Message5
    {

    }
}