using Jasper.Conneg;
using Newtonsoft.Json;

namespace Jasper.Http
{
    public class HttpSettings
    {
        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings();

        public MediaSelectionMode MediaSelectionMode { get; set; } = MediaSelectionMode.All;
    }
}
