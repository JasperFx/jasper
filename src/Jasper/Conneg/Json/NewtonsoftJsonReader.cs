using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg.Json
{
    public class NewtonsoftJsonReader : IMessageDeserializer
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

        public object ReadFromData(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                stream.Position = 0;
                return read(stream, Encoding.UTF8, DotNetType);
            }
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            var stream = request.Body;



            // TODO -- the encoding should vary here
            var user = (T) read(stream, Encoding.UTF8, typeof(T));

            return Task.FromResult(user);
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
