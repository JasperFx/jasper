using System;
using Baseline;
using Jasper.Serialization;
using Jasper.Transports;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus.Internal
{
    // SAMPLE: DefaultAzureServiceBusProtocol
    public class DefaultAzureServiceBusProtocol : ITransportProtocol<Message>
    {
        public virtual Message WriteFromEnvelope(Envelope envelope)
        {
            var message = new Message
            {
                CorrelationId = envelope.SagaId,
                MessageId = envelope.Id.ToString(),
                Body = envelope.Data,
                ContentType = envelope.ContentType,
                ReplyTo = envelope.ReplyUri?.ToString(),
                ReplyToSessionId = envelope.CausationId.ToString(),

            };

            if (envelope.ExecutionTime.HasValue)
            {
                message.ScheduledEnqueueTimeUtc = envelope.ExecutionTime.Value.UtcDateTime;
            }

            if (envelope.DeliverBy.HasValue)
            {
                message.TimeToLive = envelope.DeliverBy.Value.Subtract(DateTimeOffset.UtcNow);
            }

            EnvelopeSerializer.WriteToDictionary(message.UserProperties, envelope);



            return message;
        }

        public virtual Envelope ReadEnvelope(Message message)
        {
            var envelope = new Envelope
            {
                Id = Guid.Parse(message.MessageId),
                SagaId = message.CorrelationId,
                Data = message.Body,
                ContentType = message.ContentType,
                ReplyUri = message.ReplyTo.IsNotEmpty() ? new Uri(message.ReplyTo) : null,
                CausationId = message.ReplyToSessionId.IsNotEmpty() ? Guid.Parse(message.ReplyToSessionId) : Guid.Empty
            };


            EnvelopeSerializer.ReadPropertiesFromDictionary(message.UserProperties, envelope);

            return envelope;
        }
    }
    // ENDSAMPLE
}
