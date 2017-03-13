using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Serializers
{
    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _graph;
        private readonly Dictionary<string, IMessageSerializer> _serializers = new Dictionary<string, IMessageSerializer>();

        public EnvelopeSerializer(ChannelGraph graph, IEnumerable<IMessageSerializer> serializers)
        {
            _graph = graph;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }
        }

        public object Deserialize(Envelope envelope, ChannelNode node)
        {
            var contentType = envelope.ContentType ?? node.AcceptedContentTypes.FirstOrDefault();

            if (contentType.IsEmpty())
            {
                throw new EnvelopeDeserializationException($"No content type can be determined for {envelope}");
            }

            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                throw new EnvelopeDeserializationException($"No data on the Envelope");
            }

            // TODO -- fancier later with message mapping

            if (_serializers.ContainsKey(contentType))
            {
                var serializer = _serializers[contentType];
                using (var stream = new MemoryStream(envelope.Data))
                {
                    try
                    {
                        return serializer.Deserialize(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new EnvelopeDeserializationException("Message serializer has failed", ex);
                    }
                }
            }

            throw new EnvelopeDeserializationException($"Unknown content-type '{contentType}'");
        }

        public void Serialize(Envelope envelope, ChannelNode node)
        {
            var serializer = SelectSerializer(envelope, node);
            if (serializer == null)
            {
                throw new InvalidOperationException($"Unable to choose a serializer for {envelope} with content-type {envelope.ContentType} for channel {node.Uri}");
            }

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(envelope.Message, stream);
                stream.Position = 0;

                envelope.Data = stream.ReadAllBytes();
                envelope.ContentType = serializer.ContentType;
            }
        }

        public IMessageSerializer SelectSerializer(Envelope envelope, ChannelNode node)
        {
            if (envelope.ContentType.IsNotEmpty())
            {
                return _serializers.ContainsKey(envelope.ContentType) ? _serializers[envelope.ContentType] : null;
            }

            string mimeType = null;
            if (envelope.AcceptedContentTypes.Any())
            {
                mimeType = chooseContentType(envelope);
            }
            else if (node.AcceptedContentTypes.Any())
            {
                mimeType = chooseContentType(node);
            }
            else
            {
                mimeType = chooseContentType(_graph);
            }

            return mimeType.IsEmpty()
                ? null
                : _serializers[mimeType];
        }

        private string chooseContentType(IContentTypeAware level)
        {
            return level.Accepts.FirstOrDefault(x => _serializers.ContainsKey(x));
        }
    }
}