using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Conneg.Json
{
    public class NewtonsoftJsonWriter : IMessageSerializer
    {
        private readonly ArrayPool<char> _charPool;
        private readonly ArrayPool<byte> _bytePool;
        private readonly ObjectPool<JsonSerializer> _serializerPool;
        private readonly JsonArrayPool<char> _jsonCharPool;

        public NewtonsoftJsonWriter(Type messageType, ArrayPool<char> charPool, ArrayPool<byte> bytePool, ObjectPool<JsonSerializer> serializerPool)
            : this(messageType, "application/json", charPool, bytePool, serializerPool)
        {
        }

        public NewtonsoftJsonWriter(Type messageType, string contentType, ArrayPool<char> charPool, ArrayPool<byte> bytePool, ObjectPool<JsonSerializer> serializerPool)
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
            var bytes = _bytePool.Rent(1024); // TODO -- should this be configurable?
            var stream = new MemoryStream(bytes);

            try
            {
                using (var textWriter = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(textWriter)
                {
                    ArrayPool = _jsonCharPool,
                    CloseOutput = false,
                    //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
                })
                {
                    serializer.Serialize(jsonWriter, model);
                    return stream.ToArray();

                }
            }
            finally
            {
                _bytePool.Return(bytes);
                _serializerPool.Return(serializer);
            }
        }

        public async Task WriteToStream(object model, HttpResponse response)
        {
            using (var textWriter = new HttpResponseStreamWriter(response.Body, Encoding.UTF8, 1024, _bytePool, _charPool))
            using (var jsonWriter = new JsonTextWriter(textWriter)
            {
                ArrayPool = _jsonCharPool,
                CloseOutput = false,
                //AutoCompleteOnClose = false // TODO -- put this in if we upgrad Newtonsoft
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

            response.Headers["content-type"] = ContentType;

        }

        public Type DotNetType { get; }

    }
}
