using System;
using Jasper.Messaging.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class DefaultEnvelopeMapper : IEnvelopeMapper
    {
        public virtual Envelope ReadEnvelope(BasicDeliverEventArgs args)
        {
            var envelope = new Envelope
            {
                Data = args.Body,
                Source = args.BasicProperties.AppId,
                ContentType = args.BasicProperties.ContentType,
                MessageType = args.BasicProperties.Type
            };


            if (args.BasicProperties.Headers != null)
            {
                envelope.ReadPropertiesFromDictionary(args.BasicProperties.Headers);
            }



            return envelope;
        }

        public virtual void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            properties.CorrelationId = envelope.Id.ToString();
            properties.AppId = envelope.Source;
            properties.ContentType = envelope.ContentType;
            properties.Type = envelope.MessageType;

            envelope.WriteToDictionary(properties.Headers);
        }
    }







}
