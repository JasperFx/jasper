using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Jasper.Conneg
{

    // TODO -- This needs to be a singleton!
    public class NewtonsoftJsonWriter<T> : IMessageSerializer
    {
        // TODO -- apply character pooling here, and buffering

        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonWriter(JsonSerializer serializer)
            : this("application/json", serializer)
        {
            _serializer = _serializer;
        }

        public NewtonsoftJsonWriter(string contentType, JsonSerializer serializer)
        {
            _serializer = serializer;
            ContentType = contentType;
        }

        public string ContentType { get; }

        public byte[] Write(object model)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(new JsonTextWriter(new StreamWriter(stream){AutoFlush = true}), model);

                return stream.ToArray();
            }
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            // TODO -- there's a GH issue open to optimize this, but I'm waiting until there
            // are benchmarks in place
            _serializer.Serialize(new JsonTextWriter(new StreamWriter(response.Body){AutoFlush = true}), model);

            response.Headers["content-type"] = ContentType;

            return Task.CompletedTask;
        }

        public Type DotNetType { get; } = typeof(T);

    }
}
