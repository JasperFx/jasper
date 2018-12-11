using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Conneg.Json;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public abstract class SerializationGraph
    {
        private readonly IList<Type> _otherTypes = new List<Type>();

        private readonly IList<IMessageDeserializer> _readers = new List<IMessageDeserializer>();

        private readonly Dictionary<string, ISerializerFactory> _serializers =
            new Dictionary<string, ISerializerFactory>();

        private readonly IList<IMessageSerializer> _writers = new List<IMessageSerializer>();


        private ImHashMap<string, ModelReader> _modelReaders = ImHashMap<string, ModelReader>.Empty;
        private ImHashMap<Type, ModelWriter> _modelWriters = ImHashMap<Type, ModelWriter>.Empty;

        protected SerializationGraph(ObjectPoolProvider pooling, JsonSerializerSettings jsonSettings,
            IEnumerable<ISerializerFactory> serializers,
            IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
        {
            foreach (var serializer in serializers) _serializers.SmartAdd(serializer.ContentType, serializer);

            if (!_serializers.ContainsKey("application/json"))
            {
                var factory = new NewtonsoftSerializerFactory(jsonSettings, pooling);
                _serializers.SmartAdd("application/json", factory);
            }

            _readers.AddRange(readers);
            _writers.AddRange(writers);
        }

        public IEnumerable<ISerializerFactory> Serializers => _serializers.Values;

        public object Deserialize(Envelope envelope)
        {
            var contentType = envelope.ContentType ?? "application/json";

            if (contentType.IsEmpty())
                throw new EnvelopeDeserializationException($"No content type can be determined for {envelope}");

            if (envelope.Data == null || envelope.Data.Length == 0)
                throw new EnvelopeDeserializationException("No data on the Envelope");

            if (envelope.MessageType.IsNotEmpty())
            {
                var reader = ReaderFor(envelope.MessageType);
                if (reader.HasAnyReaders)
                    try
                    {
                        if (reader.TryRead(envelope.ContentType, envelope.Data, out var model)) return model;
                    }
                    catch (Exception ex)
                    {
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
            }

            var messageType = envelope.MessageType ?? "application/json";
            if (_serializers.ContainsKey(messageType))
                using (var stream = new MemoryStream(envelope.Data))
                {
                    stream.Position = 0;
                    return _serializers[messageType].Deserialize(stream);
                }

            throw new EnvelopeDeserializationException(
                $"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }

        public ModelWriter WriterFor(Type messageType)
        {
            if (_modelWriters.TryFind(messageType, out var writer)) return writer;

            var modelWriter = compileWriter(messageType);
            _modelWriters = _modelWriters.AddOrUpdate(messageType, modelWriter);

            return modelWriter;
        }

        private ModelWriter compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.Select(x => x.WriterFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new ModelWriter(fromSerializers.Concat(writers).ToArray());
        }

        public ModelReader ReaderFor(string messageType)
        {
            if (_modelReaders.TryFind(messageType, out var reader)) return reader;

            var modelReader = compileReader(messageType);

            _modelReaders = _modelReaders.AddOrUpdate(messageType, modelReader);

            return modelReader;
        }

        public ModelReader ReaderFor(Type inputType)
        {
            var readers = _readers.Where(x => x.DotNetType == inputType);
            var serialized = _serializers.Values.Select(x => x.ReaderFor(inputType));

            return new ModelReader(readers.Concat(serialized).ToArray());
        }

        private ModelReader compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = determineChainCandidates(messageType);

            var candidateTypes = readers.Select(x => x.DotNetType)
                .Concat(chainCandidates)
                .Concat(_otherTypes)
                .Distinct();

            var fromHandlers = candidateTypes.SelectMany(x => ReaderFor(x).Where(r => r.MessageType == messageType));


            return new ModelReader(fromHandlers.Concat(readers).Distinct().ToArray());
        }

        protected virtual IEnumerable<Type> determineChainCandidates(string messageType)
        {
            yield break;
        }

        public IMessageDeserializer JsonReaderFor(Type inputType)
        {
            return _serializers["application/json"]
                .ReaderFor(inputType);
        }

        public IMessageSerializer JsonWriterFor(Type resourceType)
        {
            return _serializers["application/json"]
                .WriterFor(resourceType);
        }

        public IMessageSerializer[] CustomWritersFor(Type resourceType)
        {
            return _writers.Where(x => x.DotNetType == resourceType).ToArray();
        }

        public IMessageDeserializer[] CustomReadersFor(Type inputType)
        {
            return _readers.Where(x => x.DotNetType == inputType).ToArray();
        }


        public void RegisterType(Type messageType)
        {
            _otherTypes.Fill(messageType);
        }
    }
}
