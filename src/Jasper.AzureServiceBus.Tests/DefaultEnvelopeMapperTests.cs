using System;
using Jasper.AzureServiceBus.Internal;
using Jasper.Messaging.Runtime;
using Jasper.Util;
using Microsoft.Azure.ServiceBus;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class DefaultEnvelopeMapperTests
    {
        public DefaultEnvelopeMapperTests()
        {
            _mapped = new Lazy<Envelope>(() =>
            {
                var mapper = new DefaultAzureServiceBusProtocol();
                var message = mapper.WriteFromEnvelope(theOriginal);

                return mapper.ReadEnvelope(message);
            });
        }

        private readonly Envelope theOriginal = new Envelope
        {
            Id = Guid.NewGuid(),
            Data = new byte[] {2, 3, 4, 5, 6}
        };

        private readonly Lazy<Envelope> _mapped;

        private readonly Message theMessage = new Message
        {
            Body = new byte[] {1, 2, 3, 4}
        };

        private Envelope theEnvelope => _mapped.Value;

        [Fact]
        public void accepted_types()
        {
            theOriginal.AcceptedContentTypes = new[] {"text/json", "text/xml"};
            theEnvelope.AcceptedContentTypes
                .ShouldHaveTheSameElementsAs(theOriginal.AcceptedContentTypes);
        }

        [Fact]
        public void ack_requestd_true()
        {
            theOriginal.AckRequested = true;
            theEnvelope.AckRequested.ShouldBeTrue();
        }

        [Fact]
        public void ack_requested_false()
        {
            theOriginal.AckRequested = false;
            theEnvelope.AckRequested.ShouldBeFalse();
        }

        [Fact]
        public void content_type()
        {
            theOriginal.ContentType = "application/json";
            theEnvelope.ContentType.ShouldBe(theOriginal.ContentType);
        }

        [Fact]
        public void deliver_by_value()
        {
            theOriginal.DeliverBy = DateTimeOffset.UtcNow.Date.AddDays(5);
            theEnvelope.DeliverBy.ShouldBe(theOriginal.DeliverBy);
        }

        [Fact]
        public void id()
        {
            theEnvelope.Id.ShouldBe(theOriginal.Id);
        }

        [Fact]
        public void map_over_the_body()
        {
            theEnvelope.Data.ShouldBe(theOriginal.Data);
        }

        [Fact]
        public void message_type()
        {
            theOriginal.MessageType = "somemessagetype";
            theEnvelope.MessageType.ShouldBe(theOriginal.MessageType);
        }

        [Fact]
        public void original_id()
        {
            theOriginal.CorrelationId = Guid.NewGuid();
            theEnvelope.CorrelationId.ShouldBe(theOriginal.CorrelationId);
        }

        [Fact]
        public void other_random_headers()
        {
            theOriginal.Headers.Add("color", "blue");
            theOriginal.Headers.Add("direction", "north");

            theEnvelope.Headers["color"].ShouldBe("blue");
            theEnvelope.Headers["direction"].ShouldBe("north");
        }

        [Fact]
        public void parent_id()
        {
            theOriginal.CausationId = Guid.NewGuid();
            theEnvelope.CausationId.ShouldBe(theOriginal.CausationId);
        }

        [Fact]
        public void reply_requested()
        {
            theOriginal.ReplyRequested = "somemessagetype";
            theEnvelope.ReplyRequested.ShouldBe(theOriginal.ReplyRequested);
        }

        [Fact]
        public void reply_uri()
        {
            theOriginal.ReplyUri = "tcp://localhost:4444".ToUri();
            theEnvelope.ReplyUri.ShouldBe(theOriginal.ReplyUri);
        }

        [Fact]
        public void saga_id()
        {
            theOriginal.SagaId = Guid.NewGuid().ToString();
            theEnvelope.SagaId.ShouldBe(theOriginal.SagaId);
        }

        [Fact]
        public void source()
        {
            theOriginal.Source = "someapp";
            theEnvelope.Source.ShouldBe(theOriginal.Source);
        }
    }


    public class mapping_from_envelope
    {
        public mapping_from_envelope()
        {
            _properties = new Lazy<Message>(() =>
            {
                return new DefaultAzureServiceBusProtocol().WriteFromEnvelope(theEnvelope);
            });
        }

        private readonly Envelope theEnvelope = new Envelope();
        private readonly Lazy<Message> _properties;

        private Message theMessage => _properties.Value;

        [Fact]
        public void ack_requested_false()
        {
            theEnvelope.AckRequested = false;
            theMessage.UserProperties.ContainsKey(Envelope.AckRequestedKey).ShouldBeFalse();
        }

        [Fact]
        public void ack_requested_true()
        {
            theEnvelope.AckRequested = true;
            theMessage.UserProperties[Envelope.AckRequestedKey].ShouldBe("true");
        }

        [Fact]
        public void content_type()
        {
            theEnvelope.ContentType = "application/json";
            theMessage.ContentType.ShouldBe(theEnvelope.ContentType);
        }

        [Fact]
        public void deliver_by()
        {
            var deliveryBy = DateTimeOffset.UtcNow.Date.AddDays(5);
            theEnvelope.DeliverBy = deliveryBy;

            var expected = deliveryBy.ToUniversalTime().Subtract(DateTime.UtcNow);
            var difference = theMessage.TimeToLive.Subtract(expected);

            difference.TotalSeconds.ShouldBeLessThan(1);
        }

        [Fact]
        public void message_type()
        {
            theEnvelope.MessageType = "something";
            theMessage.UserProperties[Envelope.MessageTypeKey].ShouldBe(theEnvelope.MessageType);
        }

        [Fact]
        public void parent_id()
        {
            theEnvelope.CausationId = Guid.NewGuid();
            theMessage.UserProperties[Envelope.ParentIdKey].ShouldBe(theEnvelope.CausationId.ToString());
        }

        [Fact]
        public void reply_requested_true()
        {
            theEnvelope.ReplyRequested = "somereplymessage";
            theMessage.UserProperties[Envelope.ReplyRequestedKey].ShouldBe(theEnvelope.ReplyRequested);
        }

        [Fact]
        public void reply_uri()
        {
            theEnvelope.ReplyUri = "tcp://localhost:5005".ToUri();
            theMessage.UserProperties[Envelope.ReplyUriKey].ShouldBe(theEnvelope.ReplyUri.ToString());
        }

        [Fact]
        public void saga_id_key()
        {
            theEnvelope.SagaId = "somesaga";
            theMessage.UserProperties[Envelope.SagaIdKey].ShouldBe(theEnvelope.SagaId);
        }

        [Fact]
        public void source_key()
        {
            theEnvelope.Source = "SomeApp";
            theMessage.UserProperties[Envelope.SourceKey].ShouldBe(theEnvelope.Source);
        }

        [Fact]
        public void the_message_id()
        {
            theEnvelope.Id = Guid.NewGuid();
            theMessage.MessageId.ShouldBe(theEnvelope.Id.ToString());
        }

        [Fact]
        public void the_original_id()
        {
            theEnvelope.CorrelationId = Guid.NewGuid();
            theMessage.UserProperties[Envelope.CorrelationIdKey].ShouldBe(theEnvelope.CorrelationId.ToString());
        }
    }
}
