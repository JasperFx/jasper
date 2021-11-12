using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using Jasper.Serialization.Json;
using Newtonsoft.Json;

namespace Jasper.Serialization.New
{
    public interface INewSerializer
    {
        string ContentType { get; }

        // TODO -- use read only memory later, and let it go back to the pool later.
        // "rent memory"
        byte[] Write(object message);

        object ReadFromData(Type messageType, byte[] data);
        object ReadFromData(byte[] data);
    }

    public class NewtonsoftSerializer : INewSerializer
    {
        private int _bufferSize = 1024;

        private readonly JsonSerializer _serializer;
        private readonly ArrayPool<byte> _bytePool;
        private readonly ArrayPool<char> _charPool;
        private readonly JsonArrayPool<char> _jsonCharPool;

        public NewtonsoftSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);

            _bytePool = ArrayPool<byte>.Shared;
            _charPool = ArrayPool<char>.Shared;
            _jsonCharPool = new JsonArrayPool<char>(_charPool);
        }

        public string ContentType { get; } = EnvelopeConstants.JsonContentType;
        public byte[] Write(object message)
        {
            var bytes = _bytePool.Rent(_bufferSize); // TODO -- should this be configurable?
            var stream = new MemoryStream(bytes);

            try
            {
                using var textWriter = new StreamWriter(stream) {AutoFlush = true};
                using var jsonWriter = new JsonTextWriter(textWriter)
                {
                    ArrayPool = _jsonCharPool,
                    CloseOutput = false

                    //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
                };

                _serializer.Serialize(jsonWriter, message);
                return stream.Position < _bufferSize
                    ? bytes.Take((int) stream.Position).ToArray()
                    : stream.ToArray();
            }

            catch (NotSupportedException e)
            {
                if (e.Message.Contains("Memory stream is not expandable"))
                {
                    var data = writeWithNoBuffer(message, _serializer);

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
            }
        }

        private byte[] writeWithNoBuffer(object model, JsonSerializer serializer)
        {
            var stream = new MemoryStream();
            using var textWriter = new StreamWriter(stream) {AutoFlush = true};
            using var jsonWriter = new JsonTextWriter(textWriter)
            {
                ArrayPool = _jsonCharPool,
                CloseOutput = false

                //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
            };

            serializer.Serialize(jsonWriter, model);
            return stream.ToArray();
        }

        public object ReadFromData(Type messageType, byte[] data)
        {
            using var stream = new MemoryStream(data)
            {
                Position = 0
            };

            using var streamReader = new StreamReader(stream, Encoding.UTF8, true,_bufferSize, true);
            using var jsonReader = new JsonTextReader(streamReader)
            {
                ArrayPool = _jsonCharPool,
                CloseInput = false
            };

            return _serializer.Deserialize(jsonReader, messageType);
        }

        public object ReadFromData(byte[] data)
        {
            using var stream = new MemoryStream(data)
            {
                Position = 0
            };

            using var streamReader = new StreamReader(stream, Encoding.UTF8, true,_bufferSize, true);
            using var jsonReader = new JsonTextReader(streamReader)
            {
                ArrayPool = _jsonCharPool,
                CloseInput = false
            };

            return _serializer.Deserialize(jsonReader);
        }
    }
}
