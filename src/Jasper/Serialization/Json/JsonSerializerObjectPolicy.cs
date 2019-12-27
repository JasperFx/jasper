using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Jasper.Serialization.Json
{
    internal class JsonSerializerObjectPolicy : IPooledObjectPolicy<JsonSerializer>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public JsonSerializerObjectPolicy(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public JsonSerializer Create()
        {
            return JsonSerializer.Create(_serializerSettings);
        }

        public bool Return(JsonSerializer serializer)
        {
            return true;
        }
    }
}
