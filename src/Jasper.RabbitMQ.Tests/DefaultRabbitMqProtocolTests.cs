using System;
using System.Collections.Generic;
using Jasper.RabbitMQ.Internal;
using Jasper.Serialization;
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
                mapper.MapEnvelopeToOutgoing(theOriginal, theEventArgs.BasicProperties);

                var envelope = new RabbitMqEnvelope(null, 0);

                mapper.MapIncomingToEnvelope(envelope, theEventArgs.BasicProperties);

                return envelope;
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
            theEnvelope.DeliverBy.HasValue.ShouldBeTrue();


            theEnvelope.DeliverBy.Value.Subtract(theOriginal.DeliverBy.Value)
                .TotalSeconds.ShouldBeLessThan(5);
        }

        [Fact]
        public void id()
        {
            theEnvelope.Id.ShouldBe(theOriginal.Id);
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
            theEnvelope.ReplyRequested.ShouldBe("somemessagetype");
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


}
