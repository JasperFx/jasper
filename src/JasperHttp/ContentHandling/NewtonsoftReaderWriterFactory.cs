using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Conneg.Json;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace JasperHttp.ContentHandling
{
    public class NewtonsoftReaderWriterFactory : ISerializerFactory<IRequestReader, IResponseWriter>
    {
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;

        public NewtonsoftReaderWriterFactory(JsonSerializerSettings settings, ObjectPoolProvider pooling)
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

        public IRequestReader ReaderFor(Type messageType)
        {
            return new NewtonsoftJsonReader(messageType, _charPool, _bytePool, _serializerPool);
        }

        public IResponseWriter WriterFor(Type messageType)
        {
            return new NewtonsoftJsonWriter(messageType, _charPool, _bytePool, _serializerPool);
        }
    }

    internal class NewtonsoftJsonReader : IRequestReader
    {
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly JsonArrayPool<char> _jsonCharPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;
        private readonly int _bufferSize = 1024;

        internal NewtonsoftJsonReader(
            Type messageType,
            ArrayPool<char> charPool,
            ArrayPool<byte> bytePool,
            ObjectPool<JsonSerializer> serializerPool
        )
            : this(messageType, charPool, bytePool, serializerPool, "application/json")
        {
        }


        internal NewtonsoftJsonReader(
            Type messageType,
            ArrayPool<char> charPool,
            ArrayPool<byte> bytePool,
            ObjectPool<JsonSerializer> serializerPool,
            string contentType)
        {
            _charPool = charPool;
            _bytePool = bytePool;


            _serializerPool = serializerPool;
            _jsonCharPool = new JsonArrayPool<char>(charPool);

            DotNetType = messageType;
            MessageType = messageType.ToMessageTypeName();
            ContentType = contentType;
        }


        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            var stream = request.Body;

            // TODO -- the encoding should vary here
            var model = (T) read(stream, Encoding.UTF8, typeof(T));

            return Task.FromResult(model);
        }

        private object read(Stream stream, Encoding encoding, Type targetType)
        {
            using (var streamReader = new HttpRequestStreamReader(stream, encoding, _bufferSize, _bytePool, _charPool))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    jsonReader.ArrayPool = _jsonCharPool;
                    jsonReader.CloseInput = false;

                    var serializer = _serializerPool.Get();

                    try
                    {
                        return serializer.Deserialize(jsonReader, targetType);
                    }
                    finally
                    {
                        _serializerPool.Return(serializer);
                    }
                }
            }
        }
    }
}
