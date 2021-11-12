using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Baseline.ImTools;
using Jasper.Util;

namespace Jasper.Serialization
{
    [Obsolete("Eliminating this soon")]
    public abstract class SerializationGraph<TReader, TWriter>
        where TReader : class, IReaderStrategy
        where TWriter : class, IWriterStrategy
    {
        private readonly IList<Type> _otherTypes = new List<Type>();

        private readonly IList<TReader> _readers = new List<TReader>();

        protected readonly Dictionary<string, ISerializerFactory<TReader, TWriter>> _serializers =
            new Dictionary<string, ISerializerFactory<TReader, TWriter>>();

        private readonly IList<TWriter> _writers = new List<TWriter>();


        private ImHashMap<string, ReaderCollection<TReader>> _modelReaders =
            ImHashMap<string, ReaderCollection<TReader>>.Empty;

        private ImHashMap<Type, WriterCollection<TWriter>> _modelWriters =
            ImHashMap<Type, WriterCollection<TWriter>>.Empty;

        protected SerializationGraph(IEnumerable<ISerializerFactory<TReader, TWriter>> serializers,
            IEnumerable<TReader> readers, IEnumerable<TWriter> writers)
        {
            foreach (var serializer in serializers)
                if (_serializers.ContainsKey(serializer.ContentType))
                    _serializers[serializer.ContentType] = serializer;
                else
                    _serializers.Add(serializer.ContentType, serializer);

            _readers.AddRange(readers);
            _writers.AddRange(writers);
        }


        public WriterCollection<TWriter> WriterFor(Type messageType)
        {
            if (_modelWriters.TryFind(messageType, out var writer)) return writer;

            var modelWriter = compileWriter(messageType);
            _modelWriters = _modelWriters.AddOrUpdate(messageType, modelWriter);

            return modelWriter;
        }

        private WriterCollection<TWriter> compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.Select(x => x.WriterFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new WriterCollection<TWriter>(fromSerializers.Concat(writers).ToArray());
        }

        public ReaderCollection<TReader> ReaderFor(string messageType)
        {
            if (_modelReaders.TryFind(messageType, out var reader)) return reader;

            var modelReader = compileReader(messageType);

            _modelReaders = _modelReaders.AddOrUpdate(messageType, modelReader);

            return modelReader;
        }

        public ReaderCollection<TReader> ReaderFor(Type inputType)
        {
            var readers = _readers.Where(x => x.DotNetType == inputType);
            var serialized = _serializers.Values.Select(x => x.ReaderFor(inputType));

            return new ReaderCollection<TReader>(readers.Concat(serialized).ToArray());
        }

        private ReaderCollection<TReader> compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = determineChainCandidates(messageType);

            var candidateTypes = readers.Select(x => x.DotNetType)
                .Concat(chainCandidates)
                .Concat(_otherTypes)
                .Distinct();

            var fromHandlers = candidateTypes.SelectMany(x => ReaderFor(x).Where(r => r.MessageType == messageType));


            return new ReaderCollection<TReader>(fromHandlers.Concat(readers).Distinct().ToArray());
        }

        protected virtual IEnumerable<Type> determineChainCandidates(string messageType)
        {
            yield break;
        }

        public TReader JsonReaderFor(Type inputType)
        {
            return _serializers[EnvelopeConstants.JsonContentType]
                .ReaderFor(inputType);
        }

        public TWriter JsonWriterFor(Type resourceType)
        {
            return _serializers[EnvelopeConstants.JsonContentType]
                .WriterFor(resourceType);
        }

        public TWriter[] CustomWritersFor(Type resourceType)
        {
            return _writers.Where(x => x.DotNetType == resourceType).ToArray();
        }

        public TReader[] CustomReadersFor(Type inputType)
        {
            return _readers.Where(x => x.DotNetType == inputType).ToArray();
        }


        public void RegisterType(Type messageType)
        {
            _otherTypes.Fill(messageType);
        }
    }
}
