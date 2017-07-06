using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftJsonWriter<T> : IMediaWriter
    {
        // TODO -- apply character pooling here

        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonWriter(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        public string ContentType { get; } = "application/json";

        public byte[] Write(object model)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(new JsonTextWriter(new StreamWriter(stream)), model);
                return stream.ToArray();
            }
        }

        public Task Write(object model, Stream stream)
        {
            _serializer.Serialize(new JsonTextWriter(new StreamWriter(stream)), model);
            return Task.CompletedTask;
        }

        public Type DotNetType { get; } = typeof(T);

    }
}
