using System;
using System.Reflection;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Util;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Messaging.Transports.Tcp
{
    public class serialization_and_deserialization_of_single_message
    {
        public serialization_and_deserialization_of_single_message()
        {
            outgoing = new Envelope
            {
                SentAt = DateTime.Today.ToUniversalTime(),
                Data = new byte[] {1, 5, 6, 11, 2, 3},
                Destination = "durable://localhost:2222/incoming".ToUri(),
                DeliverBy = DateTime.Today.ToUniversalTime(),
                ReplyUri = "durable://localhost:2221/replies".ToUri(),
                SagaId = Guid.NewGuid().ToString()
            };


            sentAttempts = typeof(Envelope).GetProperty("SentAttempts", BindingFlags.Instance | BindingFlags.NonPublic);
            sentAttempts.SetValue(outgoing, 2);

            outgoing.Headers.Add("name", "Jeremy");
            outgoing.Headers.Add("state", "Texas");
        }

        private readonly Envelope outgoing;
        private Envelope _incoming;
        private readonly PropertyInfo sentAttempts;

        private Envelope incoming
        {
            get
            {
                if (_incoming == null)
                {
                    var messageBytes = outgoing.Serialize();
                    _incoming = Envelope.Deserialize(messageBytes);
                }

                return _incoming;
            }
        }

        [Fact]
        public void accepted_content_types_positive()
        {
            outgoing.AcceptedContentTypes = new[] {"a", "b"};
            incoming.AcceptedContentTypes.ShouldHaveTheSameElementsAs("a", "b");
        }

        [Fact]
        public void ack_requested_negative()
        {
            outgoing.AckRequested = false;
            incoming.AckRequested.ShouldBeFalse();
        }

        [Fact]
        public void ack_requested_positive()
        {
            outgoing.AckRequested = true;
            incoming.AckRequested.ShouldBeTrue();
        }

        [Fact]
        public void all_the_headers()
        {
            incoming.Headers["name"].ShouldBe("Jeremy");
            incoming.Headers["state"].ShouldBe("Texas");
        }

        [Fact]
        public void brings_over_the_correlation_id()
        {
            incoming.Id.ShouldBe(outgoing.Id);
        }

        [Fact]
        public void brings_over_the_saga_id()
        {
            incoming.SagaId.ShouldBe(outgoing.SagaId);
        }

        [Fact]
        public void content_type()
        {
            outgoing.ContentType = "application/json";
            incoming.ContentType.ShouldBe(outgoing.ContentType);
        }

        [Fact]
        public void data_comes_over()
        {
            incoming.Data.ShouldHaveTheSameElementsAs(outgoing.Data);
        }


        [Fact]
        public void deliver_by_with_value()
        {
            incoming.DeliverBy.Value.ShouldBe(outgoing.DeliverBy.Value);
        }

        [Fact]
        public void deliver_by_without_value()
        {
            outgoing.DeliverBy = null;
            incoming.DeliverBy.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void destination()
        {
            incoming.Destination.ShouldBe(outgoing.Destination);
        }

        [Fact]
        public void execution_time_not_null()
        {
            outgoing.ExecutionTime = DateTime.Today;
            incoming.ExecutionTime.ShouldBe(DateTime.Today.ToUniversalTime());
        }

        [Fact]
        public void execution_time_null()
        {
            outgoing.ExecutionTime = null;
            incoming.ExecutionTime.HasValue.ShouldBeFalse();
        }

        [Fact]
        public void message_type()
        {
            outgoing.MessageType = "some.model.object";

            incoming.MessageType.ShouldBe(outgoing.MessageType);
        }

        [Fact]
        public void original_id()
        {
            outgoing.CorrelationId = Guid.NewGuid();
            incoming.CorrelationId.ShouldBe(outgoing.CorrelationId);
        }

        [Fact]
        public void parent_id()
        {
            outgoing.CausationId = Guid.NewGuid();
            incoming.CausationId.ShouldBe(outgoing.CausationId);
        }

        [Fact]
        public void received_at()
        {
            outgoing.ReceivedAt = "http://server1".ToUri();
            incoming.ReceivedAt.ShouldBe(outgoing.ReceivedAt);
        }

        [Fact]
        public void reply_requested()
        {
            outgoing.ReplyRequested = Guid.NewGuid().ToString();
            incoming.ReplyRequested.ShouldBe(outgoing.ReplyRequested);
        }

        [Fact]
        public void reply_uri()
        {
            incoming.ReplyUri.ShouldBe(outgoing.ReplyUri);
        }

        [Fact]
        public void sent_at()
        {
            incoming.SentAt.ShouldBe(outgoing.SentAt);
        }


        [Fact]
        public void sent_attempts()
        {
            sentAttempts.GetValue(incoming).As<int>().ShouldBe(2);
        }

        [Fact]
        public void source()
        {
            outgoing.Source = "something";
            incoming.Source.ShouldBe(outgoing.Source);
        }
    }
}
