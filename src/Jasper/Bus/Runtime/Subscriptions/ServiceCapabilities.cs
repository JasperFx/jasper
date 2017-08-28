using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public abstract class JsonPersistable<T>
    {
        public void WriteToFile(string file)
        {
            var json = ToJson();
            new FileSystem().WriteStringToFile(file, json);
        }

        public string ToJson()
        {
            var settings = serializationSettings();

            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            return json;
        }

        private static JsonSerializerSettings serializationSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()},
                TypeNameHandling = TypeNameHandling.None
            };
            return settings;
        }

        public static T ReadFromFile(string file)
        {
            var settings = serializationSettings();
            var json = new FileSystem().ReadStringFromFile(file);

            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }

    public class ServiceCapabilities : JsonPersistable<ServiceCapabilities>
    {
        public string ServiceName { get; set; }

        public PublishedMessage[] Published { get; set; } = new PublishedMessage[0];

        public Subscription[] Subscriptions { get; set; } = new Subscription[0];

        public string[] Errors { get; set; } = new string[0];

        public bool Publishes<T>()
        {
            return Published.Any(x => x.DotNetType == typeof(T));
        }

        public async Task ApplyLookups(UriAliasLookup lookups)
        {
            var all = Subscriptions.Select(x => x.Destination).Where(x => x != null).Distinct().ToArray();
            await lookups.ReadAliases(all);

            foreach (var subscription in Subscriptions.Where(x => x.Destination != null))
            {
                subscription.Destination = lookups.Resolve(subscription.Destination);
            }
        }


    }
}
