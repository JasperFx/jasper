using Newtonsoft.Json;

namespace Jasper.Remotes.Messaging
{
    public abstract class ClientMessage
    {
        [JsonProperty("type")]
        public string Type { get; private set; }

        protected ClientMessage(string type)
        {
            Type = type;
        }
    }
}
