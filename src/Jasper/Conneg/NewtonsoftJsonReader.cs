using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftJsonReader<T> : IMediaReader
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonReader(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }

        public string MessageType { get; } = typeof(T).ToTypeAlias();
        public Type DotNetType { get; } = typeof(T);
        public string ContentType { get; } = "application/json";

        public object Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                stream.Position = 0;
                return _serializer.Deserialize<T>(new JsonTextReader(new StreamReader(stream)));
            }


        }

        public Task<T1> Read<T1>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
