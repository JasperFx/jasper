using System;
using Baseline;
using Jasper.Messaging.Runtime;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus
{
    public class DefaultEnvelopeMapper : IEnvelopeMapper
    {
        public Message WriteFromEnvelope(Envelope envelope)
        {
            var message = new Message
            {
                CorrelationId = envelope.SagaId,
                MessageId = envelope.Id.ToString(),
                Body = envelope.Data,
                ContentType = envelope.ContentType,
                ReplyTo = envelope.ReplyUri?.ToString(),
                ReplyToSessionId = envelope.ParentId.ToString(),





            };

            if (envelope.DeliverBy.HasValue)
            {
                message.TimeToLive = envelope.DeliverBy.Value.Subtract(DateTimeOffset.UtcNow);
            }



            envelope.WriteToDictionary(message.UserProperties);



            return message;
        }

        public Envelope ReadEnvelope(Message message)
        {
            var envelope = new Envelope
            {
                Id = Guid.Parse(message.MessageId),
                SagaId = message.CorrelationId,
                Data = message.Body,
                ContentType = message.ContentType,
                ReplyUri = message.ReplyTo.IsNotEmpty() ? new Uri(message.ReplyTo) : null,
                ParentId = message.ReplyToSessionId.IsNotEmpty() ? Guid.Parse(message.ReplyToSessionId) : Guid.Empty
            };


            envelope.ReadPropertiesFromDictionary(message.UserProperties);

            return envelope;
        }
    }
}
