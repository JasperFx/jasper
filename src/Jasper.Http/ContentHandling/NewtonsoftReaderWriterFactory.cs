using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Serialization;
using Jasper.Serialization.Json;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Http.ContentHandling
{
    public interface IResponseWriter : IWriterStrategy
    {
        /// <summary>
        ///     Called during HTTP requests
        /// </summary>
        /// <param name="model"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task WriteToStream(object model, HttpResponse response);
    }


    public interface IRequestReader : IReaderStrategy
    {
        Task<T> ReadFromRequest<T>(HttpRequest request);
    }

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

        public string ContentType => EnvelopeConstants.JsonContentType;

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
        private readonly int _bufferSize = 1024;
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly JsonArrayPool<char> _jsonCharPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;

        internal NewtonsoftJsonReader(
            Type messageType,
            ArrayPool<char> charPool,
            ArrayPool<byte> bytePool,
            ObjectPool<JsonSerializer> serializerPool
        )
            : this(messageType, charPool, bytePool, serializerPool, EnvelopeConstants.JsonContentType)
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

        public async Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            // TODO -- the encoding should vary here
            var targetType = typeof(T);
            object ret;

            if (!request.Body.CanSeek)
            {
                // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously
                // read everything into a buffer, and then seek back to the beginning.
                request.EnableBuffering();

                await request.Body.DrainAsync(CancellationToken.None);
                request.Body.Seek(0L, SeekOrigin.Begin);
            }

            using (var streamReader =
                new HttpRequestStreamReader(request.Body, Encoding.UTF8, _bufferSize, _bytePool, _charPool))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    jsonReader.ArrayPool = _jsonCharPool;
                    jsonReader.CloseInput = false;


                    var serializer = _serializerPool.Get();

                    try
                    {
                        ret = serializer.Deserialize(jsonReader, targetType);
                    }
                    finally
                    {
                        _serializerPool.Return(serializer);
                    }
                }
            }

            return (T) ret;
        }
    }


    public class NewtonsoftJsonWriter : IResponseWriter
    {
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly JsonArrayPool<char> _jsonCharPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;
        private int _bufferSize = 1024;

        public NewtonsoftJsonWriter(Type messageType, ArrayPool<char> charPool, ArrayPool<byte> bytePool,
            ObjectPool<JsonSerializer> serializerPool)
            : this(messageType, EnvelopeConstants.JsonContentType, charPool, bytePool, serializerPool)
        {
        }

        public NewtonsoftJsonWriter(Type messageType, string contentType, ArrayPool<char> charPool,
            ArrayPool<byte> bytePool, ObjectPool<JsonSerializer> serializerPool)
        {
            DotNetType = messageType;
            ContentType = contentType;
            _charPool = charPool;
            _bytePool = bytePool;
            _serializerPool = serializerPool;
            _jsonCharPool = new JsonArrayPool<char>(charPool);
        }

        public string ContentType { get; }

        public Type DotNetType { get; }

        public async Task WriteToStream(object model, HttpResponse response)
        {
            response.Headers["content-type"] = ContentType;

            using (var textWriter =
                new HttpResponseStreamWriter(response.Body, Encoding.UTF8, 1024, _bytePool, _charPool))
            using (var jsonWriter = new JsonTextWriter(textWriter)
            {
                ArrayPool = _jsonCharPool,
                CloseOutput = false,
                AutoCompleteOnClose = false
            })
            {
                var serializer = _serializerPool.Get();

                try
                {
                    serializer.Serialize(jsonWriter, model);
                    await textWriter.FlushAsync();
                }
                finally
                {
                    _serializerPool.Return(serializer);
                }
            }
        }

        public byte[] Write(object model)
        {
            var serializer = _serializerPool.Get();
            var bytes = _bytePool.Rent(_bufferSize); // TODO -- should this be configurable?
            var stream = new MemoryStream(bytes);

            try
            {
                using (var textWriter = new StreamWriter(stream) {AutoFlush = true})
                using (var jsonWriter = new JsonTextWriter(textWriter)
                {
                    ArrayPool = _jsonCharPool,
                    CloseOutput = false

                    //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
                })
                {
                    serializer.Serialize(jsonWriter, model);
                    if (stream.Position < _bufferSize)
                    {
                        return bytes.Take((int) stream.Position).ToArray();
                    }

                    return stream.ToArray();
                }
            }

            catch (NotSupportedException e)
            {
                if (e.Message.Contains("Memory stream is not expandable"))
                {
                    var data = writeWithNoBuffer(model, serializer);

                    var bufferSize = 1024;
                    while (bufferSize < data.Length)
                    {
                        bufferSize = bufferSize * 2;
                    }

                    _bufferSize = bufferSize;

                    return data;
                }

                throw;
            }

            finally
            {
                _bytePool.Return(bytes);
                _serializerPool.Return(serializer);
            }
        }

        private byte[] writeWithNoBuffer(object model, JsonSerializer serializer)
        {
            var stream = new MemoryStream();
            using (var textWriter = new StreamWriter(stream) {AutoFlush = true})
            using (var jsonWriter = new JsonTextWriter(textWriter)
            {
                ArrayPool = _jsonCharPool,
                CloseOutput = false

                //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
            })
            {
                serializer.Serialize(jsonWriter, model);
                return stream.ToArray();
            }
        }
    }
}
