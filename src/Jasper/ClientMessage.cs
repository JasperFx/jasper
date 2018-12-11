using Newtonsoft.Json;

namespace Jasper
{
    /// <summary>
    ///     Base class for messages that need to embed the message type directly
    ///     into the message payload. Originally for WebSockets work
    /// </summary>
    public abstract class ClientMessage
    {
        protected ClientMessage(string type)
        {
            Type = type;
        }

        [JsonProperty("type")] public string Type { get; private set; }
    }
}
