using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Util;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftJsonReader<T> : IMediaReader where T : class
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonReader(JsonSerializer serializer)
            : this("application/json", serializer)
        {

        }


        public NewtonsoftJsonReader(string contentType, JsonSerializer serializer)
        {
            _serializer = serializer;
            DotNetType = typeof(T);
            MessageType = typeof(T).ToTypeAlias();
            ContentType = contentType;
        }



        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

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
