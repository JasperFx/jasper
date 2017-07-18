using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Jasper.Bus.Runtime;
using Newtonsoft.Json;

namespace Jasper.Consul
{
    // TODO -- clean this code up and eliminate the duplication. It's at spike quality code at the moment
    public interface IConsulGateway
    {
        Task<string> GetProperty(string key);
        Task SetProperty(string key, string value);
    }

    public class ConsulGateway : IConsulGateway
    {
        private readonly HttpClient _client;
        private readonly ConsulSettings _settings;
        private readonly ConsulClient _consul;

        public ConsulGateway(HttpClient client, ConsulSettings settings)
        {
            _client = client;
            _settings = settings;
            _consul = new ConsulClient(settings.Configure);
        }


        public async Task<string> GetProperty(string key)
        {
            var result = await _consul.KV.Get(key);
            var data = result.Response.Value;
            return Encoding.UTF8.GetString(data);
        }

        public Task SetProperty(string key, string value)
        {
            return _consul.KV.Put(new KVPair(key)
            {
                Value = Encoding.UTF8.GetBytes(value)
            });
        }

    }
}
