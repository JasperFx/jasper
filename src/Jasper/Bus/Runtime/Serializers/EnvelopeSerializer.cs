using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Serializers
{
    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _channels;
        private readonly HandlerGraph _handlers;
        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        private readonly IList<IMediaReader> _readers = new List<IMediaReader>();
        private readonly IList<IMediaWriter> _writers = new List<IMediaWriter>();

        public EnvelopeSerializer(ChannelGraph channels, HandlerGraph handlers, IEnumerable<ISerializer> serializers, IEnumerable<IMediaReader> readers, IEnumerable<IMediaWriter> writers)
        {
            _channels = channels;
            _handlers = handlers;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }

            _readers.AddRange(readers);
            _writers.AddRange(writers);
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

        public ISerializer SelectSerializer(Envelope envelope, ChannelNode node)
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
            // It's perfectly possible to not have a matching, static ChannelNode here
            else if (node != null && node.AcceptedContentTypes.Any())
            {
                mimeType = chooseContentType(node);
            }
            else
            {
                mimeType = chooseContentType(_channels);
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
