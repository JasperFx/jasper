using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Serialization;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    // SAMPLE: DefaultRabbitMqProtocol
    public class DefaultRabbitMqProtocol : IRabbitMqProtocol
    {
        public virtual void ReadIntoEnvelope(Envelope envelope, IBasicProperties props, byte[] data)
        {
            envelope.Data = data;
            envelope.Source = props.AppId;
            envelope.ContentType = props.ContentType;
            envelope.MessageType = props.Type;
            envelope.ReplyUri = props.ReplyTo.IsEmpty() ? null : new Uri(props.ReplyTo);


            if (Guid.TryParse(props.CorrelationId, out var id))
            {
                envelope.Id = id;
            }

            if (props.Headers != null)
            {
                EnvelopeSerializer.ReadPropertiesFromDictionary(props.Headers, envelope);
            }
        }

        public virtual void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            properties.CorrelationId = envelope.Id.ToString();
            properties.AppId = envelope.Source;
            properties.ContentType = envelope.ContentType;
            properties.Type = envelope.MessageType;
            if (envelope.ReplyUri != null) properties.ReplyTo = envelope.ReplyUri.ToString();

            properties.Headers ??= new Dictionary<string, object>();

            EnvelopeSerializer.WriteToDictionary(properties.Headers, envelope);
        }
    }
    // ENDSAMPLE
}
