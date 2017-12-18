using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg.Json;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public abstract class SerializationGraph
    {
        private readonly MediaSelectionMode _selectionMode;
        private readonly Forwarders _forwarders;
        private readonly Dictionary<string, ISerializerFactory> _serializers = new Dictionary<string, ISerializerFactory>();

        private readonly IList<IMessageDeserializer> _readers = new List<IMessageDeserializer>();
        private readonly IList<IMessageSerializer> _writers = new List<IMessageSerializer>();

        private readonly ConcurrentDictionary<string, ModelReader> _modelReaders = new ConcurrentDictionary<string, ModelReader>();
        private readonly ConcurrentDictionary<Type, ModelWriter> _modelWriters = new ConcurrentDictionary<Type, ModelWriter>();
        private readonly IList<Type> _otherTypes = new List<Type>();

        protected SerializationGraph(ObjectPoolProvider pooling, MediaSelectionMode selectionMode, JsonSerializerSettings jsonSettings, Forwarders forwarders, IEnumerable<ISerializerFactory> serializers, IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
        {
            _selectionMode = selectionMode;
            _forwarders = forwarders;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }

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

            var messageType = envelope.MessageType ?? "application/json";
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
            var fromSerializers = _serializers.Values.SelectMany(x => x.WritersFor(messageType, _selectionMode));
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
            var serialized = _serializers.Values.SelectMany(x => x.ReadersFor(inputType, _selectionMode));

            var forwarded = _forwarders.ForwardingTypesTo(inputType).SelectMany(incomingType =>
            {
                return _serializers.Values.Select(x =>
                {
                    var inner = x.VersionedReaderFor(incomingType);
                    return typeof(ForwardingMessageDeserializer<>).CloseAndBuildAs<IMessageDeserializer>(inner, inputType);
                });
            });


            return new ModelReader(readers.Concat(serialized).Concat(forwarded).ToArray());
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
                .ReadersFor(inputType, _selectionMode)
                .FirstOrDefault(x => x.ContentType == "application/json");
        }

        public IMessageSerializer JsonWriterFor(Type resourceType)
        {
            return _serializers["application/json"]
                .WritersFor(resourceType, _selectionMode)
                .FirstOrDefault(x => x.ContentType == "application/json");
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
