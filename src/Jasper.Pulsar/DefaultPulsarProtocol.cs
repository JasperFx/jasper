using System.Buffers;
using System.Linq;
using Baseline;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Serialization;

namespace Jasper.Pulsar
{
    public class DefaultPulsarProtocol : IPulsarProtocol
    {
        public void WriteFromEnvelope(Envelope env, MessageMetadata message)
        {
            message[EnvelopeSerializer.SourceKey] = env.Source;
            message[EnvelopeSerializer.MessageTypeKey] = env.MessageType;
            if (env.ReplyUri is not null)
            {
                message[EnvelopeSerializer.ReplyUriKey] = env.ReplyUri.ToString();
            }

            message[EnvelopeSerializer.ContentTypeKey] = env.ContentType;
            message[EnvelopeSerializer.CorrelationIdKey] = env.CorrelationId.ToString();
            message[EnvelopeSerializer.CausationIdKey] = env.CausationId.ToString();

            if (env.SagaId.IsNotEmpty())
            {
                message[EnvelopeSerializer.SagaIdKey] = env.SagaId;
            }

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                message[EnvelopeSerializer.AcceptedContentTypesKey] = string.Join(",", env.AcceptedContentTypes);
            }

            message[EnvelopeSerializer.IdKey] = env.Id.ToString();
            if (env.ReplyRequested != null)
            {
                message[EnvelopeSerializer.ReplyRequestedKey] = env.ReplyRequested.ToString();
            }

            message[EnvelopeSerializer.AckRequestedKey] = env.AckRequested.ToString();

            if (env.ExecutionTime.HasValue)
            {
                message.DeliverAtTimeAsDateTimeOffset = env.ExecutionTime.Value;
            }

            message[EnvelopeSerializer.AttemptsKey] = env.Attempts.ToString();

            if (env.DeliverBy.HasValue)
            {
                message[EnvelopeSerializer.DeliverByHeader] = env.DeliverBy.Value.ToString("o");
            }

            foreach (var pair in env.Headers)
            {
                message[pair.Key] = pair.Value;
            }
        }

        public void ReadIntoEnvelope(Envelope envelope, IMessage<ReadOnlySequence<byte>> message)
        {
            EnvelopeSerializer.ReadPropertiesFromDictionary(message.Properties, envelope);
        }
    }
}
