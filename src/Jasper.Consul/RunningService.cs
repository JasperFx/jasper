using Newtonsoft.Json;

namespace Jasper.Consul
{
    /// <summary>
    /// AgentServiceRegistration is used to register a new service
    /// </summary>
    public class RunningService
    {
        public RunningService()
        {
        }

        // TODO -- later, have it take in ChannelGraph and build from there.
        public RunningService(string id, string name)
        {
            ID = id;
            Name = name;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Port { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }
    }
}