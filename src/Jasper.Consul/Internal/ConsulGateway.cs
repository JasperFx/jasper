using System;
using System.Text;
using System.Threading.Tasks;
using Consul;

namespace Jasper.Consul.Internal
{
    public interface IConsulGateway
    {
        Task<string> GetProperty(string key);
        Task SetProperty(string key, string value);
    }

    public class ConsulGateway : IConsulGateway, IDisposable
    {
        private readonly ConsulClient _consul;

        public ConsulGateway(ConsulSettings settings)
        {
            _consul = settings.Client;
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

        public void Dispose()
        {
            _consul?.Dispose();
        }
    }
}
