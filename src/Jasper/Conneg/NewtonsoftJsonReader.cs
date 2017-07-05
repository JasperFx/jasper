using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftJsonReader<T> : IMediaReader<T>
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonReader(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        public string ContentType { get; } = "application/json";

        public T Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                stream.Position = 0;
                return _serializer.Deserialize<T>(new JsonTextReader(new StreamReader(stream)));
            }


        }

        public Task<T> Read(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}