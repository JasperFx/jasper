using System;
using System.Buffers;
using System.IO;
using System.Linq;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Serialization.Json
{
    internal class NewtonsoftJsonWriter : IMessageSerializer
    {
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly JsonArrayPool<char> _jsonCharPool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;
        private int _bufferSize = 1024;

        public NewtonsoftJsonWriter(Type messageType, ArrayPool<char> charPool, ArrayPool<byte> bytePool,
            ObjectPool<JsonSerializer> serializerPool)
            : this(messageType, "application/json", charPool, bytePool, serializerPool)
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

        public byte[] Write(object model)
        {
            var serializer = _serializerPool.Get();
            var bytes = _bytePool.Rent(_bufferSize);
            var stream = new MemoryStream(bytes);

            try
            {
                using (var textWriter = new StreamWriter(stream) {AutoFlush = true})
                using (var jsonWriter = new JsonTextWriter(textWriter)
                {
                    ArrayPool = _jsonCharPool,
                    CloseOutput = false,
                    AutoCompleteOnClose = false

                })
                {
                    serializer.Serialize(jsonWriter, model);
                    if (stream.Position < _bufferSize) return bytes.Take((int) stream.Position).ToArray();

                    return stream.ToArray();
                }
            }

            catch (NotSupportedException e)
            {
                if (e.Message.Contains("Memory stream is not expandable"))
                {
                    var data = writeWithNoBuffer(model, serializer);

                    var bufferSize = 1024;
                    while (bufferSize < data.Length) bufferSize = bufferSize * 2;

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

        public Type DotNetType { get; }

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
