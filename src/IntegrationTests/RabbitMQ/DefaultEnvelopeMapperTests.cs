using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using Jasper.RabbitMQ;
using Jasper.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public class DefaultEnvelopeMapperTests
    {
        private Envelope theOriginal = new Envelope
        {
            Id = Guid.NewGuid()
        };
        private Lazy<Envelope> _mapped;
        private BasicDeliverEventArgs theEventArgs = new BasicDeliverEventArgs
        {
            BasicProperties = new BasicProperties
            {
                Headers = new Dictionary<string, object>()
            },
            Body = new byte[]{1,2,3,4}
        };

        public DefaultEnvelopeMapperTests()
        {
            _mapped = new Lazy<Envelope>(() =>
            {
                var mapper = new DefaultEnvelopeMapper();
                mapper.WriteFromEnvelope(theOriginal, theEventArgs.BasicProperties);

                return mapper.ReadEnvelope(theEventArgs.Body, theEventArgs.BasicProperties);


            });
        }

        private Envelope theEnvelope => _mapped.Value;

        [Fact]
        public void map_over_the_body()
        {
            theEnvelope.Data.ShouldBe(theEventArgs.Body);
        }

        [Fact]
        public void id()
        {
            theEnvelope.Id.ShouldBe(theOriginal.Id);
        }

        [Fact]
        public void source()
        {
            theOriginal.Source = "someapp";
            theEnvelope.Source.ShouldBe(theOriginal.Source);
        }

        [Fact]
        public void message_type()
        {
            theOriginal.MessageType = "somemessagetype";
            theEnvelope.MessageType.ShouldBe(theOriginal.MessageType);
        }

        [Fact]
        public void reply_uri()
        {
            theOriginal.ReplyUri = "tcp://localhost:4444".ToUri();
            theEnvelope.ReplyUri.ShouldBe(theOriginal.ReplyUri);
        }

        [Fact]
        public void content_type()
        {
            theOriginal.ContentType = "application/json";
            theEnvelope.ContentType.ShouldBe(theOriginal.ContentType);
        }

        [Fact]
        public void original_id()
        {
            theOriginal.OriginalId = Guid.NewGuid();
            theEnvelope.OriginalId.ShouldBe(theOriginal.OriginalId);
        }

        [Fact]
        public void parent_id()
        {
            theOriginal.ParentId = Guid.NewGuid();
            theEnvelope.ParentId.ShouldBe(theOriginal.ParentId);
        }

        [Fact]
        public void response_id()
        {
            theOriginal.ResponseId = Guid.NewGuid();
            theEnvelope.ResponseId.ShouldBe(theOriginal.ResponseId);
        }

        [Fact]
        public void saga_id()
        {
            theOriginal.SagaId = Guid.NewGuid().ToString();
            theEnvelope.SagaId.ShouldBe(theOriginal.SagaId);
        }

        [Fact]
        public void reply_requested()
        {
            theOriginal.ReplyRequested = "somemessagetype";
            theEnvelope.ReplyRequested.ShouldBe(theOriginal.ReplyRequested);
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
        public void deliver_by_value()
        {
            theOriginal.DeliverBy = DateTimeOffset.Now;
            theEnvelope.DeliverBy.ShouldBe(theOriginal.DeliverBy);
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
        public void accepted_types()
        {
            theOriginal.AcceptedContentTypes = new string[]{"text/json", "text/xml"};
            theEnvelope.AcceptedContentTypes
                .ShouldHaveTheSameElementsAs(theOriginal.AcceptedContentTypes);
        }
    }


    public class mapping_from_envelope
    {
        private Envelope theEnvelope = new Envelope();
        private Lazy<IBasicProperties> _properties;

        public mapping_from_envelope()
        {
            _properties = new Lazy<IBasicProperties>(() =>
            {
                var props = new BasicProperties
                {
                    Headers = new Dictionary<string, object>()
                };
                new DefaultEnvelopeMapper().WriteFromEnvelope(theEnvelope, props);

                return props;
            });
        }

        private IBasicProperties theProperties => _properties.Value;

        [Fact]
        public void content_type()
        {
            theEnvelope.ContentType = "application/json";
            theProperties.ContentType.ShouldBe(theEnvelope.ContentType);
        }

        [Fact]
        public void the_correlation_id()
        {
            theEnvelope.Id = Guid.NewGuid();
            theProperties.CorrelationId.ShouldBe(theEnvelope.Id.ToString());
        }

        [Fact]
        public void message_type()
        {
            theEnvelope.MessageType = "something";
            theProperties.Type.ShouldBe(theEnvelope.MessageType);
        }

        [Fact]
        public void source_key()
        {
            theEnvelope.Source = "SomeApp";
            theProperties.AppId.ShouldBe(theEnvelope.Source);
        }

        [Fact]
        public void reply_uri()
        {
            theEnvelope.ReplyUri = "tcp://localhost:5005".ToUri();
            theProperties.Headers[Envelope.ReplyUriKey].ShouldBe(theEnvelope.ReplyUri.ToString());
        }

        [Fact]
        public void the_original_id()
        {
            theEnvelope.OriginalId = Guid.NewGuid();
            theProperties.Headers[Envelope.OriginalIdKey].ShouldBe(theEnvelope.OriginalId.ToString());
        }

        [Fact]
        public void parent_id()
        {
            theEnvelope.ParentId = Guid.NewGuid();
            theProperties.Headers[Envelope.ParentIdKey].ShouldBe(theEnvelope.ParentId.ToString());
        }

        [Fact]
        public void response_id_key()
        {
            theEnvelope.ResponseId = Guid.NewGuid();
            theProperties.Headers[Envelope.ResponseIdKey].ShouldBe(theEnvelope.ResponseId.ToString());
        }

        [Fact]
        public void saga_id_key()
        {
            theEnvelope.SagaId = "somesaga";
            theProperties.Headers[Envelope.SagaIdKey].ShouldBe(theEnvelope.SagaId);
        }

        [Fact]
        public void reply_requested_true()
        {
            theEnvelope.ReplyRequested = "somereplymessage";
            theProperties.Headers[Envelope.ReplyRequestedKey].ShouldBe(theEnvelope.ReplyRequested);
        }

        [Fact]
        public void ack_requested_true()
        {
            theEnvelope.AckRequested = true;
            theProperties.Headers[Envelope.AckRequestedKey].ShouldBe("true");
        }

        [Fact]
        public void ack_requested_false()
        {
            theEnvelope.AckRequested = false;
            theProperties.Headers.ContainsKey(Envelope.AckRequestedKey).ShouldBeFalse();
        }


    }
}
