using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftJsonReader<T> : IMessageDeserializer where T : class
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
            MessageType = typeof(T).ToMessageAlias();
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

                return _serializer.Deserialize<T>(new JsonTextReader(new StreamReader(stream)));
            }


        }

        public Task<T1> ReadFromRequest<T1>(HttpRequest request)
        {
            // TODO -- this'll get changed w/ the JSON optimization
            var model = _serializer.Deserialize<T1>(new JsonTextReader(new StreamReader(request.Body)));
            return Task.FromResult(model);
        }
    }
}
