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
        private readonly ArrayPool<char> _charPool;
        private readonly ArrayPool<byte> _bytePool;
        private readonly JsonArrayPool<char> _jsonCharPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;

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
            MessageType = messageType.ToMessageAlias();
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

        public async Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            var stream = request.Body;

            if (!stream.CanSeek)
            {
                // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously
                // read everything into a buffer, and then seek back to the beginning.
                request.EnableRewind();

                await stream.DrainAsync(CancellationToken.None);
                stream.Seek(0L, SeekOrigin.Begin);
            }

            // TODO -- the encoding should vary here
            return (T) read(stream, Encoding.UTF8, typeof(T));
        }

        private object read(Stream stream, Encoding encoding, Type targetType)
        {
            using (var streamReader = new HttpRequestStreamReader(stream, encoding, 1024, _bytePool, _charPool))
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
