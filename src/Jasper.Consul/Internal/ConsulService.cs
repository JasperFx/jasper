using System.Text;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Newtonsoft.Json;

namespace Jasper.Consul.Internal
{
    public abstract class ConsulService
    {
        protected const string GLOBAL_PREFIX = "jasper/";

        private readonly IChannelGraph _channels;


        protected ConsulService(ConsulSettings settings, IChannelGraph channels, BusSettings envSettings)
        {
            _channels = channels;
            client = settings.Client;
            MachineName = envSettings.MachineName;
        }

        protected ConsulClient client { get; }

        public string ServiceName => _channels.Name;

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
