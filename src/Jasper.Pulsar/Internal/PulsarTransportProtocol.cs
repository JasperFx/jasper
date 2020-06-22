using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using DotPulsar;
using Jasper.Pulsar.Internal;
using Jasper.Transports;

namespace Jasper.Pulsar
{
    internal class PulsarTransportProtocol : ITransportProtocol<PulsarMessage>
    {
        public const string PulsarMessageKeyHeader = "Pulsar.Message.Key";
        public const string PulsarMessageSequenceIdHeader = "Pulsar.Message.SequenceId";

        private readonly Dictionary<string, Action<MessageMetadata, object>> _pulsarMsgPropTypes = new Dictionary<string, Action<MessageMetadata, object>>()
        {
            { PulsarMessageKeyHeader, (metadata, val) => metadata.Key = val?.ToString() },
            { PulsarMessageSequenceIdHeader, (metadata, val) => metadata.SequenceId = val != null ? ulong.Parse(val.ToString()) : default },
        };

        public PulsarMessage WriteFromEnvelope(Envelope envelope)
        {
            IDictionary<string, object> envelopHeaders = new Dictionary<string, object>();
            envelope.WriteToDictionary(envelopHeaders);

            var metadata = new MessageMetadata();
            
            foreach (var header in envelopHeaders.Where(h => !_pulsarMsgPropTypes.Keys.Contains(h.Key)))
            {
                metadata[header.Key] = header.Value.ToString();
            }

            if (envelope.ExecutionTime.HasValue)
            {
                metadata.DeliverAtTimeAsDateTimeOffset = envelope.ExecutionTime.Value;
            }

            SetMetaDataFromHeaderValues(metadata, envelopHeaders);

            return new PulsarMessage(envelope.Data, metadata);
        }

        private void SetMetaDataFromHeaderValues(MessageMetadata metadata, IDictionary<string, object> envelopHeaders)
        {
            foreach (var pulsarMsgPropType in _pulsarMsgPropTypes)
            {
                SetMetaDataFromHeaderValue(metadata, envelopHeaders, pulsarMsgPropType.Key, pulsarMsgPropType.Value);
            }
        }

        private void SetMetaDataFromHeaderValue(MessageMetadata metadata, IDictionary<string, object> envelopHeaders, string propertyName, Action<MessageMetadata, object> propertySetter)
        {
            if (envelopHeaders.TryGetValue(propertyName, out object headerValue)) propertySetter(metadata, headerValue);
        }

        public Envelope ReadEnvelope(PulsarMessage message)
        {
            var envelope = new Envelope
            {
                Data = message.Data.ToArray(),
                Headers = message.Properties.ToDictionary(ks => ks.Key, vs => vs.Value)
            };

            envelope.ReadPropertiesFromDictionary(message.Properties.ToDictionary(ks => ks.Key, vs => (object)vs.Value));

            return envelope;
        }
    }
}
