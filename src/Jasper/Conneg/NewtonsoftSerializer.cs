using System;
using System.IO;
using Baseline;
using Jasper.Bus;
using Newtonsoft.Json;

namespace Jasper.Conneg
{
    public class NewtonsoftSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public NewtonsoftSerializer(BusSettings settings)
        {
            //settings.TypeNameHandling = TypeNameHandling.Objects;
            _serializer = Newtonsoft.Json.JsonSerializer.Create(settings.JsonSerialization);
        }

        public void Serialize(object message, Stream stream)
        {
            var writer = new StreamWriter(stream);
            _serializer.Serialize(writer, message);
            writer.Flush();
        }

        public object Deserialize(Stream message)
        {
            var reader = new JsonTextReader(new StreamReader(message));
            return _serializer.Deserialize(reader);
        }

        public string ContentType => "application/json";
        public IMediaReader[] ReadersFor(Type messageType)
        {
            // TODO -- this will be more later when we get the versions sorted out

            return new IMediaReader[]
            {
                typeof(NewtonsoftJsonReader<>).CloseAndBuildAs<IMediaReader>(_serializer, messageType)
            };
        }

        public IMediaWriter[] WritersFor(Type messageType)
        {
            // TODO -- this will be more later when we get the versions sorted out
            throw new NotImplementedException();
        }
    }
}
