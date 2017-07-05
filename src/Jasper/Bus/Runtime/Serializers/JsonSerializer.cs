using System.IO;
using Jasper.Conneg;
using Newtonsoft.Json;

namespace Jasper.Bus.Runtime.Serializers
{
    public class JsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer(JsonSerializerSettings settings)
        {
            //settings.TypeNameHandling = TypeNameHandling.Objects;
            _serializer = Newtonsoft.Json.JsonSerializer.Create(settings);
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
    }
}
