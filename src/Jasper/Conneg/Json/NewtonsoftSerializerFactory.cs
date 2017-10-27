using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg.Json
{
    // SAMPLE: NewtonsoftSerializer
    public class NewtonsoftSerializerFactory : ISerializerFactory
    {
        private readonly ObjectPool<JsonSerializer> _serializerPool;
        private readonly ArrayPool<char> _charPool;
        private readonly ArrayPool<byte> _bytePool;

        public NewtonsoftSerializerFactory(JsonSerializerSettings settings, ObjectPoolProvider pooling)
        {
            _serializerPool = pooling.Create(new JsonSerializerObjectPolicy(settings));

            _charPool = ArrayPool<char>.Shared;
            _bytePool = ArrayPool<byte>.Shared;
        }

        public object Deserialize(Stream message)
        {
            var serializer = _serializerPool.Get();
            try
            {
                var reader = new JsonTextReader(new StreamReader(message))
                {
                    ArrayPool = new JsonArrayPool<char>(_charPool),
                    CloseInput = true
                };

                return serializer.Deserialize(reader);
            }
            finally
            {
                _serializerPool.Return(serializer);
            }
        }

        public string ContentType => "application/json";

        private IEnumerable<IMessageDeserializer> determineReaders(Type messageType, MediaSelectionMode mode)
        {
            if (mode == MediaSelectionMode.All)
            {
                yield return new NewtonsoftJsonReader(messageType, _charPool, _bytePool, _serializerPool);
            }

            if (messageType.HasAttribute<VersionAttribute>() || mode == MediaSelectionMode.VersionedOnly)
            {
                yield return VersionedReaderFor(messageType);
            }
        }

        public IMessageDeserializer[] ReadersFor(Type messageType, MediaSelectionMode mode)
        {
            return determineReaders(messageType, mode).ToArray();
        }

        public IMessageSerializer[] WritersFor(Type messageType, MediaSelectionMode mode)
        {
            return determineWriters(messageType, mode).ToArray();
        }

        public IMessageDeserializer VersionedReaderFor(Type incomingType)
        {
            return new NewtonsoftJsonReader(incomingType, _charPool, _bytePool, _serializerPool, incomingType.ToContentType("json"));
        }

        private IEnumerable<IMessageSerializer> determineWriters(Type messageType, MediaSelectionMode mode)
        {
            if (mode == MediaSelectionMode.All)
            {
                yield return new NewtonsoftJsonWriter(messageType, _charPool, _bytePool, _serializerPool);
            }

            if (messageType.HasAttribute<VersionAttribute>() || mode == MediaSelectionMode.VersionedOnly)
            {
                var contentType = messageType.ToContentType("json");
                yield return new NewtonsoftJsonWriter(messageType, contentType, _charPool, _bytePool, _serializerPool);
            }
        }
    }
    // ENDSAMPLE
}
