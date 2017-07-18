using Consul;
using Jasper.Bus.Runtime;

namespace Jasper.Consul
{
    public class ConsulSettings
    {
        public int Port { get; set; } = 8500;

        internal void Configure(ConsulClientConfiguration config)
        {
            config.Address = $"http://localhost:{Port}".ToUri();
        }
    }
}
