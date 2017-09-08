using System;
using Consul;
using Jasper.Bus.Runtime;
using Jasper.Util;

namespace Jasper.Consul
{
    public class ConsulSettings : IDisposable
    {
        private readonly Lazy<ConsulClient> _client;

        public ConsulSettings()
        {
            _client = new Lazy<ConsulClient>(() => new ConsulClient(configure));
        }

        public int Port { get; set; } = 8500;

        private void configure(ConsulClientConfiguration config)
        {
            config.Address = $"http://localhost:{Port}".ToUri();
        }

        internal ConsulClient Client => _client.Value;

        public void Dispose()
        {
            if (_client.IsValueCreated)
            {
                _client.Value.Dispose();
            }
        }
    }
}
