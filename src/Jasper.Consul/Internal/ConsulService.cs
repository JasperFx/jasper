using System.Text;
using Consul;
using Jasper.Messaging.Transports.Configuration;
using Newtonsoft.Json;

namespace Jasper.Consul.Internal
{
    public abstract class ConsulService
    {
        protected const string GlobalPrefix = "jasper/";


        protected ConsulService(ConsulSettings settings, MessagingSettings envSettings)
        {
            client = settings.Client;
            MachineName = envSettings.MachineName;
        }

        protected ConsulClient client { get; }


        public string MachineName { get; }

        protected static byte[] serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }

        protected static T deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
    }
}
