using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using DotPulsar;
using Jasper.Pulsar.Internal;
using Jasper.Transports;
using Newtonsoft.Json;

namespace Jasper.Pulsar
{
    internal class PulsarTransportProtocol : ITransportProtocol<PulsarMessage>
    {
        public const string PulsarMessageKeyHeader = "Pulsar.Message.Key";
        public const string PulsarMessageSequenceIdHeader = "Pulsar.Message.SequenceId";
        public const string PulsarMessageIdHeader = "Pulsar.Message.MessageId";

        private readonly Dictionary<string, Action<PulsarMessage, object>> _pulsarMsgPropTypes = new Dictionary<string, Action<PulsarMessage, object>>()
        {
            { PulsarMessageKeyHeader, (msg, val) => msg.Metadata.Key = val?.ToString() },
            { PulsarMessageSequenceIdHeader, (msg, val) => msg.Metadata.SequenceId = val != null ? ulong.Parse(val.ToString()) : default },
            { PulsarMessageIdHeader, (msg, val) => msg.MessageId = val != null ? JsonConvert.DeserializeObject<MessageId>(val.ToString()) : null },
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

            var pulsarMessage = new PulsarMessage(envelope.Data, metadata);
            SetMetaDataFromHeaderValues(pulsarMessage, envelopHeaders);
            return pulsarMessage;
        }

        private void SetMetaDataFromHeaderValues(PulsarMessage msg, IDictionary<string, object> envelopHeaders)
        {
            foreach (KeyValuePair<string, Action<PulsarMessage, object>> pulsarMsgPropType in _pulsarMsgPropTypes)
            {
                SetMetaDataFromHeaderValue(msg, envelopHeaders, pulsarMsgPropType.Key, pulsarMsgPropType.Value);
            }
        }

        private void SetMetaDataFromHeaderValue(PulsarMessage msg, IDictionary<string, object> envelopHeaders, string propertyName, Action<PulsarMessage, object> propertySetter)
        {
            if (envelopHeaders.TryGetValue(propertyName, out object headerValue)) propertySetter(msg, headerValue);
        }

        public Envelope ReadEnvelope(PulsarMessage message)
        {
            var envelope = new Envelope
            {  
                Data = message.Data.ToArray(),
                Headers = message.Properties.ToDictionary(ks => ks.Key, vs => vs.Value),
            };

            envelope.Headers.Add(PulsarMessageIdHeader, JsonConvert.SerializeObject(message.MessageId));
            
            envelope.ReadPropertiesFromDictionary(message.Properties.ToDictionary(ks => ks.Key, vs => (object)vs.Value));

            return envelope;
        }
    }
}
