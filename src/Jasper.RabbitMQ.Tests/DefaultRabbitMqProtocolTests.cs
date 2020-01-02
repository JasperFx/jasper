using System;
using System.Collections.Generic;
using Jasper.RabbitMQ.Internal;
using Jasper.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class DefaultRabbitMqProtocolTests
    {
        public DefaultRabbitMqProtocolTests()
        {
            _mapped = new Lazy<Envelope>(() =>
            {
                var mapper = new DefaultRabbitMqProtocol();
                mapper.WriteFromEnvelope(theOriginal, theEventArgs.BasicProperties);

                return mapper.ReadEnvelope(theEventArgs.Body, theEventArgs.BasicProperties);
            });
        }

        private readonly Envelope theOriginal = new Envelope
        {
            Id = Guid.NewGuid(),

        };

        private readonly Lazy<Envelope> _mapped;

        private readonly BasicDeliverEventArgs theEventArgs = new BasicDeliverEventArgs
        {
            BasicProperties = new BasicProperties
            {
                Headers = new Dictionary<string, object>()
            },
            Body = new byte[] {1, 2, 3, 4},

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
            theOriginal.DeliverBy = DateTimeOffset.Now;
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
            theEnvelope.Data.ShouldBe(theEventArgs.Body);
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
            _properties = new Lazy<IBasicProperties>(() =>
            {
                var props = new BasicProperties
                {
                    Headers = new Dictionary<string, object>()
                };
                new DefaultRabbitMqProtocol().WriteFromEnvelope(theEnvelope, props);

                return props;
            });
        }

        private readonly Envelope theEnvelope = new Envelope();
        private readonly Lazy<IBasicProperties> _properties;

        private IBasicProperties theProperties => _properties.Value;

        [Fact]
        public void ack_requested_false()
        {
            theEnvelope.AckRequested = false;
            theProperties.Headers.ContainsKey(Envelope.AckRequestedKey).ShouldBeFalse();
        }

        [Fact]
        public void ack_requested_true()
        {
            theEnvelope.AckRequested = true;
            theProperties.Headers[Envelope.AckRequestedKey].ShouldBe("true");
        }

        [Fact]
        public void content_type()
        {
            theEnvelope.ContentType = "application/json";
            theProperties.ContentType.ShouldBe(theEnvelope.ContentType);
        }

        [Fact]
        public void message_type()
        {
            theEnvelope.MessageType = "something";
            theProperties.Type.ShouldBe(theEnvelope.MessageType);
        }

        [Fact]
        public void parent_id()
        {
            theEnvelope.CausationId = Guid.NewGuid();
            theProperties.Headers[Envelope.ParentIdKey].ShouldBe(theEnvelope.CausationId.ToString());
        }

        [Fact]
        public void reply_requested_true()
        {
            theEnvelope.ReplyRequested = "somereplymessage";
            theProperties.Headers[Envelope.ReplyRequestedKey].ShouldBe(theEnvelope.ReplyRequested);
        }

        [Fact]
        public void reply_uri()
        {
            theEnvelope.ReplyUri = "tcp://localhost:5005".ToUri();
            theProperties.Headers[Envelope.ReplyUriKey].ShouldBe(theEnvelope.ReplyUri.ToString());
        }

        [Fact]
        public void saga_id_key()
        {
            theEnvelope.SagaId = "somesaga";
            theProperties.Headers[Envelope.SagaIdKey].ShouldBe(theEnvelope.SagaId);
        }

        [Fact]
        public void source_key()
        {
            theEnvelope.Source = "SomeApp";
            theProperties.AppId.ShouldBe(theEnvelope.Source);
        }

        [Fact]
        public void the_correlation_id()
        {
            theEnvelope.Id = Guid.NewGuid();
            theProperties.CorrelationId.ShouldBe(theEnvelope.Id.ToString());
        }

        [Fact]
        public void the_original_id()
        {
            theEnvelope.CorrelationId = Guid.NewGuid();
            theProperties.Headers[Envelope.CorrelationIdKey].ShouldBe(theEnvelope.CorrelationId.ToString());
        }
    }
}
