using System;
using System.Buffers;
using System.IO;
using Jasper.Configuration;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg.Json
{
    // SAMPLE: NewtonsoftSerializer
    public class NewtonsoftSerializerFactory : ISerializerFactory<IMessageDeserializer, IMessageSerializer>
    {
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;

        public NewtonsoftSerializerFactory(AdvancedSettings settings, ObjectPoolProvider pooling)
        {
            _serializerPool = pooling.Create(new JsonSerializerObjectPolicy(settings.JsonSerialization));

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

        public IMessageDeserializer ReaderFor(Type messageType)
        {
            return new NewtonsoftJsonReader(messageType, _charPool, _bytePool, _serializerPool);
        }

        public IMessageSerializer WriterFor(Type messageType)
        {
            return new NewtonsoftJsonWriter(messageType, _charPool, _bytePool, _serializerPool);
        }
    }

    // ENDSAMPLE
}
