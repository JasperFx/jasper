using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Util;

namespace Jasper.Conneg
{
    public class SerializationGraph
    {
        public static SerializationGraph Basic()
        {
            return new SerializationGraph(new HandlerGraph(), new List<ISerializer>{new NewtonsoftSerializer(new BusSettings())}, new List<IMediaReader>(), new List<IMediaWriter>());
        }

        private readonly HandlerGraph _handlers;
        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        private readonly IList<IMediaReader> _readers = new List<IMediaReader>();
        private readonly IList<IMediaWriter> _writers = new List<IMediaWriter>();

        private readonly ConcurrentDictionary<string, ModelReader> _modelReaders = new ConcurrentDictionary<string, ModelReader>();
        private readonly ConcurrentDictionary<Type, ModelWriter> _modelWriters = new ConcurrentDictionary<Type, ModelWriter>();

        public SerializationGraph(HandlerGraph handlers, IEnumerable<ISerializer> serializers, IEnumerable<IMediaReader> readers, IEnumerable<IMediaWriter> writers)
        {
            _handlers = handlers;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }

            _readers.AddRange(readers);
            _writers.AddRange(writers);
        }

        public IEnumerable<ISerializer> Serializers => _serializers.Values;

        public object Deserialize(Envelope envelope, ChannelNode node)
        {
            var contentType = envelope.ContentType ?? node.AcceptedContentTypes.FirstOrDefault() ?? "application/json";

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
                var reader = ReaderFor(envelope.MessageType);
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

            var messageType = envelope.MessageType ?? node.AcceptedContentTypes.FirstOrDefault() ?? "application/json";
            if (_serializers.ContainsKey(messageType))
            {
                using (var stream = new MemoryStream(envelope.Data))
                {
                    stream.Position = 0;
                    return _serializers[messageType].Deserialize(stream);
                }
            }

            throw new EnvelopeDeserializationException($"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }

        public ModelWriter WriterFor(Type messageType)
        {
            return _modelWriters.GetOrAdd(messageType, compileWriter);
        }

        private ModelWriter compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.SelectMany(x => x.WritersFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new ModelWriter(fromSerializers.Concat(writers).ToArray());
        }

        public ModelReader ReaderFor(string messageType)
        {
            return _modelReaders.GetOrAdd(messageType, compileReader);
        }

        public ModelReader ReaderFor(Type inputType)
        {
            var readers = _readers.Where(x => x.DotNetType == inputType);
            var serialized = _serializers.Values.SelectMany(x => x.ReadersFor(inputType));

            return new ModelReader(readers.Concat(serialized).ToArray());
        }

        private ModelReader compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = _handlers.Chains.Where(x => x.MessageType.ToTypeAlias() == messageType)
                .Select(x => x.MessageType);

            var candidateTypes = _readers.Select(x => x.DotNetType).Concat(chainCandidates).Distinct();

            var fromSerializers =
                _serializers.Values.SelectMany(x => candidateTypes.SelectMany(messageType1 => x.ReadersFor(messageType1)));

            return new ModelReader(fromSerializers.Concat(readers).ToArray());
        }

        public IMediaReader JsonReaderFor(Type inputType)
        {
            return _serializers["application/json"]
                .ReadersFor(inputType)
                .FirstOrDefault(x => x.ContentType == "application/json");
        }

        public IMediaWriter JsonWriterFor(Type resourceType)
        {
            return _serializers["application/json"]
                .WritersFor(resourceType)
                .FirstOrDefault(x => x.ContentType == "application/json");
        }

        public IMediaWriter[] CustomWritersFor(Type resourceType)
        {
            return _writers.Where(x => x.DotNetType == resourceType).ToArray();
        }

        public IMediaReader[] CustomReadersFor(Type inputType)
        {
            return _readers.Where(x => x.DotNetType == inputType).ToArray();
        }


    }
}
