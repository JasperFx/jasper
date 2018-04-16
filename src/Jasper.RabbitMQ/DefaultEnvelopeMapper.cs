using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class DefaultEnvelopeMapper : IEnvelopeMapper
    {
        public virtual Envelope ReadEnvelope(byte[] data, IBasicProperties props)
        {
            var envelope = new Envelope
            {
                Data = data,
                Source = props.AppId,
                ContentType = props.ContentType,
                MessageType = props.Type
            };


            if (props.Headers != null)
            {
                envelope.ReadPropertiesFromDictionary(props.Headers);
            }



            return envelope;
        }

        public virtual void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            properties.CorrelationId = envelope.Id.ToString();
            properties.AppId = envelope.Source;
            properties.ContentType = envelope.ContentType;
            properties.Type = envelope.MessageType;

            if (properties.Headers == null) properties.Headers = new Dictionary<string, object>();

            envelope.WriteToDictionary(properties.Headers);
        }
    }







}
