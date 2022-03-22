using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Jasper.Serialization;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Runtime.Routing
{
    public class StaticRoute : IMessageRoute
    {
        private readonly ISendingAgent? _agent;
        private readonly MessageTypeRouting _routing;
        private readonly Endpoint _endpoint;
        private readonly IMessageSerializer? _writer;

        public StaticRoute(ISendingAgent? agent,
            MessageTypeRouting routing)
        {
            _agent = agent;
            _endpoint = agent.Endpoint;
            _routing = routing;

            _writer = _endpoint.DefaultSerializer ?? throw new InvalidOperationException($"The endpoint at '{_endpoint.Uri}' does not have any known serializers");

        }


        public void Configure(Envelope? envelope)
        {
            if (envelope.ContentType.IsNotEmpty())
            {
                envelope.Serializer = _endpoint.TryFindSerializer(envelope.ContentType);
            }
            else
            {
                envelope.Serializer = _writer;
                envelope.ContentType = _writer?.ContentType;
            }

            envelope.Sender = _agent;
            envelope.Destination = _agent.Destination;

            applyEnvelopeRules(envelope);
        }

        private void applyEnvelopeRules(Envelope? envelope)
        {
            _endpoint.Customize(envelope);
            foreach (var customization in _routing.Customizations)
            {
                customization(envelope);
            }
        }

        public Envelope? CloneForSending(Envelope? envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");

            var writer = envelope.ContentType.IsEmpty() ? _writer : _endpoint.TryFindSerializer(envelope.ContentType);

            if (writer == null)
            {
                throw new ArgumentOutOfRangeException(
                    $"Requested serializer for content-type '{envelope.ContentType}' cannot be found on endpoint '{_endpoint.Uri}");
            }

            var sending = envelope.CloneForWriter(writer);

            sending.Id = CombGuidIdGeneration.NewGuid();
            sending.CorrelationId = envelope.Id.ToString();

            sending.ReplyUri = envelope.ReplyUri ?? _agent.ReplyUri;

            sending.ContentType = _writer.ContentType;

            sending.Destination = _agent.Destination;

            sending.Sender = _agent;

            applyEnvelopeRules(sending);

            return sending;
        }



        public Envelope? BuildForSending(object? message)
        {
            var envelope = new Envelope(message)
            {
                Serializer = _writer,
                ContentType = _writer.ContentType,
                Sender = _agent,
                Destination = _agent.Destination
            };

            applyEnvelopeRules(envelope);

            return envelope;
        }

        public Uri? Destination => _agent.Destination;
        public string? ContentType => _writer.ContentType;
    }
}
