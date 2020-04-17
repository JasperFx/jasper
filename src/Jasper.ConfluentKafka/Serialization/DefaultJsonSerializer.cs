using System;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Newtonsoft.Json;

namespace Jasper.ConfluentKafka.Serialization
{
    internal class DefaultJsonSerializer<T> : IAsyncSerializer<T>
    {
        public Task<byte[]> SerializeAsync(T data, SerializationContext context)
        {
            var json = JsonConvert.SerializeObject(data);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }

        public ISerializer<T> AsSyncOverAsync()
        {
            return new SyncOverAsyncSerializer<T>(this);
        }
    }

    internal class DefaultJsonDeserializer<T> : IAsyncDeserializer<T>
    {
        public IDeserializer<T> AsSyncOverAsync()
        {
            return new SyncOverAsyncDeserializer<T>(this);
        }

        public Task<T> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data.ToArray())));
        }
    }
}
