using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Serializers
{
    public class EnvelopeSerializer : IEnvelopeSerializer
    {
        private readonly ChannelGraph _channels;
        private readonly HandlerGraph _handlers;
        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        private readonly IList<IMediaReader> _readers = new List<IMediaReader>();
        private readonly IList<IMediaWriter> _writers = new List<IMediaWriter>();

        private readonly ConcurrentDictionary<string, ModelReader> _modelReaders = new ConcurrentDictionary<string, ModelReader>();
        private readonly ConcurrentDictionary<Type, ModelWriter> _modelWriters = new ConcurrentDictionary<Type, ModelWriter>();

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

        private ModelWriter writerFor(Type messageType)
        {
            return _modelWriters.GetOrAdd(messageType, compileWriter);
        }

        private ModelWriter compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.SelectMany(x => x.WritersFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new ModelWriter(fromSerializers.Concat(writers).ToArray());
        }

        private ModelReader readerFor(string messageType)
        {
            return _modelReaders.GetOrAdd(messageType, compileReader);
        }

        private ModelReader compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = _handlers.Chains.Where(x => x.MessageType.ToTypeAlias() == messageType)
                .Select(x => x.MessageType);

            var candidateTypes = _readers.Select(x => x.DotNetType).Concat(chainCandidates).Distinct();

            var fromSerializers =
                _serializers.Values.SelectMany(x => candidateTypes.SelectMany(x.ReadersFor));

            return new ModelReader(fromSerializers.Concat(readers).ToArray());
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

            if (envelope.MessageType.IsNotEmpty())
            {
                var reader = readerFor(envelope.MessageType);
                if (reader.HasAnyReaders)
                {
                    try
                    {
                        if (reader.TryRead(envelope.ContentType, envelope.Data, out object model))
                        {
                            return model;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
                }
            }

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
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
                }
            }

            throw new EnvelopeDeserializationException($"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }

        public void Serialize(Envelope envelope, ChannelNode node)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message cannot be null");
            }

            var writer = writerFor(envelope.Message.GetType());

            // TODO -- change the node AcceptedContentTypes to an accepts string instead
            if (writer.TryWrite(envelope.Accepts ?? node.AcceptedContentTypes.Join(","), envelope.Message, out string contentType, out byte[] data))
            {
                envelope.Data = data;
                envelope.ContentType = contentType;

                return;
            }

            throw new InvalidOperationException($"Unable to choose a serializer for {envelope} with content-type {envelope.ContentType} and message type {envelope.Message.GetType().FullName} for channel {node.Uri}");
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
