using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Newtonsoft.Json;

namespace Jasper.Consul
{
    // TODO -- clean this code up and eliminate the duplication. It's at spike quality code at the moment
    public interface IConsulGateway
    {
        Task RegisterService(ServiceRegistration registration);
        Task<ServiceRegistration[]> GetRegisteredServices();
        Task<ServiceRegistration[]> GetRegisteredServices(string serviceName);
        Task<string> GetProperty(string key);
        Task SetProperty(string key, string value);
        Task Register(RunningService service);
        Task UnRegister(string id);
    }

    public class ConsulGateway : IConsulGateway
    {
        private readonly HttpClient _client;
        private readonly ConsulSettings _settings;

        public ConsulGateway(HttpClient client, ConsulSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        public Task RegisterService(ServiceRegistration registration)
        {
            var json = JsonConvert.SerializeObject(registration);
            var bytes = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(bytes);

            return _client.PutAsync($"http://localhost:{_settings.Port}/v1/catalog/register",
                content);
        }

        public async Task<ServiceRegistration[]> GetRegisteredServices()
        {
            var json = await _client.GetStringAsync($"http://localhost:{_settings.Port}/v1/catalog/nodes");
            var raw = JsonConvert.DeserializeObject<ServiceRegistration[]>(json);
            return raw.Where(x => x.ServiceName != ServiceRegistration.Unknown).ToArray();
        }

        public async Task<ServiceRegistration[]> GetRegisteredServices(string serviceName)
        {
            var json = await _client.GetStringAsync($"http://localhost:{_settings.Port}/v1/catalog/nodes/{serviceName}");
            var raw = JsonConvert.DeserializeObject<ServiceRegistration[]>(json);
            return raw.Where(x => x.ServiceName != ServiceRegistration.Unknown).ToArray();
        }

        public async Task<string> GetProperty(string key)
        {
            var json = await _client.GetStringAsync($"http://localhost:{_settings.Port}/v1/kv/{key}");
            var response = JsonConvert.DeserializeObject<KeyValueResponse[]>(json);

            return Encoding.UTF8.GetString(response.First().Value);
        }

        public Task SetProperty(string key, string value)
        {
            var content = new StringContent(value);
            return _client.PutAsync($"http://localhost:{_settings.Port}/v1/kv/{key}", content);
        }

        public Task Register(RunningService service)
        {
            var json = JsonConvert.SerializeObject(service);
            var bytes = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(bytes);

            return _client.PutAsync($"http://localhost:{_settings.Port}/v1/agent/service/register",
                    content);
        }

        public Task UnRegister(string id)
        {
            return _client.PutAsync($"http://localhost:{_settings.Port}/v1/agent/service/deregister/{id}",
                new StringContent(id));
        }

    }
}
