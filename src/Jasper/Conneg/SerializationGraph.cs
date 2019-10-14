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

        protected readonly Dictionary<string, ISerializerFactory> _serializers =
            new Dictionary<string, ISerializerFactory>();

        private readonly IList<IMessageSerializer> _writers = new List<IMessageSerializer>();


        private ImHashMap<string, ReaderCollection> _modelReaders = ImHashMap<string, ReaderCollection>.Empty;
        private ImHashMap<Type, WriterCollection> _modelWriters = ImHashMap<Type, WriterCollection>.Empty;

        protected SerializationGraph(IEnumerable<ISerializerFactory> serializers,
            IEnumerable<IMessageDeserializer> readers, IEnumerable<IMessageSerializer> writers)
        {
            foreach (var serializer in serializers)
            {
                if (_serializers.ContainsKey(serializer.ContentType))
                {
                    _serializers[serializer.ContentType] = serializer;
                }
                else
                {
                    _serializers.Add(serializer.ContentType, serializer);
                }
            }

            _readers.AddRange(readers);
            _writers.AddRange(writers);
        }


        public WriterCollection WriterFor(Type messageType)
        {
            if (_modelWriters.TryFind(messageType, out var writer)) return writer;

            var modelWriter = compileWriter(messageType);
            _modelWriters = _modelWriters.AddOrUpdate(messageType, modelWriter);

            return modelWriter;
        }

        private WriterCollection compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.Select(x => x.WriterFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new WriterCollection(fromSerializers.Concat(writers).ToArray());
        }

        public ReaderCollection ReaderFor(string messageType)
        {
            if (_modelReaders.TryFind(messageType, out var reader)) return reader;

            var modelReader = compileReader(messageType);

            _modelReaders = _modelReaders.AddOrUpdate(messageType, modelReader);

            return modelReader;
        }

        public ReaderCollection ReaderFor(Type inputType)
        {
            var readers = _readers.Where(x => x.DotNetType == inputType);
            var serialized = _serializers.Values.Select(x => x.ReaderFor(inputType));

            return new ReaderCollection(readers.Concat(serialized).ToArray());
        }

        private ReaderCollection compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = determineChainCandidates(messageType);

            var candidateTypes = readers.Select(x => x.DotNetType)
                .Concat(chainCandidates)
                .Concat(_otherTypes)
                .Distinct();

            var fromHandlers = candidateTypes.SelectMany(x => ReaderFor(x).Where(r => r.MessageType == messageType));


            return new ReaderCollection(fromHandlers.Concat(readers).Distinct().ToArray());
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
