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
        public MessageMetadata WriteFromEnvelope(Envelope env)
        {
            var metadata = new MessageMetadata();

            metadata[EnvelopeSerializer.SourceKey] = env.Source;
            metadata[EnvelopeSerializer.MessageTypeKey] = env.MessageType;
            if (env.ReplyUri is not null)
            {
                metadata[EnvelopeSerializer.ReplyUriKey] = env.ReplyUri.ToString();
            }

            metadata[EnvelopeSerializer.ContentTypeKey] = env.ContentType;
            metadata[EnvelopeSerializer.CorrelationIdKey] = env.CorrelationId.ToString();
            metadata[EnvelopeSerializer.CausationIdKey] = env.CausationId.ToString();

            if (env.SagaId.IsNotEmpty())
            {
                metadata[EnvelopeSerializer.SagaIdKey] = env.SagaId;
            }

            if (env.AcceptedContentTypes != null && env.AcceptedContentTypes.Any())
            {
                metadata[EnvelopeSerializer.AcceptedContentTypesKey] = string.Join(",", env.AcceptedContentTypes);
            }

            metadata[EnvelopeSerializer.IdKey] = env.Id.ToString();
            if (env.ReplyRequested != null)
            {
                metadata[EnvelopeSerializer.ReplyRequestedKey] = env.ReplyRequested.ToString();
            }

            metadata[EnvelopeSerializer.AckRequestedKey] = env.AckRequested.ToString();

            if (env.ExecutionTime.HasValue)
            {
                metadata.DeliverAtTimeAsDateTimeOffset = env.ExecutionTime.Value;
            }

            metadata[EnvelopeSerializer.AttemptsKey] = env.Attempts.ToString();

            if (env.DeliverBy.HasValue)
            {
                metadata[EnvelopeSerializer.DeliverByHeader] = env.DeliverBy.Value.ToString("o");
            }

            foreach (var pair in env.Headers)
            {
                metadata[pair.Key] = pair.Value;
            }
            return metadata;
        }

        public void ReadIntoEnvelope(Envelope envelope, IMessage<ReadOnlySequence<byte>> message)
        {
            // TODO -- find a way to make the serializer work off of
            // the read only sequence instead of a byte[]
            envelope.Data = message.Data.ToArray();

            EnvelopeSerializer.ReadPropertiesFromDictionary(message.Properties, envelope);
        }
    }
}
