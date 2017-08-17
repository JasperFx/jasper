using Newtonsoft.Json;

namespace Jasper.Bus
{
    public class BusSettings
    {
        public int ResponsePort { get; set; } = 2333;
        public int MaximumFireAndForgetSendingAttempts { get; set; } = 3;
        public bool DisableAllTransports { get; set; }

        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}
