﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using DotPulsar.Abstractions;
using DotPulsar.Internal.PulsarApi;
using Jasper.Serialization;
using Jasper.Util;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using Shouldly;
using TestingSupport;
using Xunit;
using MessageMetadata = DotPulsar.MessageMetadata;

namespace Jasper.Pulsar.Tests
{
    public class DefaultPulsarProtocolTests
    {
        public DefaultPulsarProtocolTests()
        {
            _mapped = new Lazy<Envelope>(() =>
            {
                var mapper = new DefaultPulsarProtocol();
                var metadata = mapper.WriteFromEnvelope(theOriginal);

                var prop1 = typeof(MessageMetadata).GetProperty("Metadata",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                var internalMetadata = prop1.GetValue(metadata);
                var prop2 = internalMetadata.GetType().GetProperty("Properties");
                var values = (List<KeyValue>) prop2.GetValue(internalMetadata);

                var properties = new Dictionary<string, string>();
                foreach (var pair in values)
                {
                    properties[pair.Key] = pair.Value;
                }

                var message = Substitute.For<IMessage<ReadOnlySequence<byte>>>();
                message.Properties.Returns(properties);

                var envelope = new PulsarEnvelope(message);

                mapper.ReadIntoEnvelope(envelope, message);

                return envelope;
            });
        }

        private readonly Envelope theOriginal = new Envelope
        {
            Id = Guid.NewGuid(),

        };

        private readonly Lazy<Envelope> _mapped;


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
