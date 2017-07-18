using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Runtime;

namespace Jasper.Consul.Internal
{
    public class ConsulUriLookup : IUriLookup
    {
        private readonly IConsulGateway _gateway;

        public ConsulUriLookup(IConsulGateway gateway)
        {
            _gateway = gateway;
        }

        public string Protocol { get; } = "consul";
        public async Task<Uri[]> Lookup(Uri[] originals)
        {
            var actuals = new Uri[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                var key = originals[i].Host;
                var actual = await _gateway.GetProperty(key);

                if (actual.IsEmpty())
                {
                    throw new ArgumentOutOfRangeException(nameof(key), $"No known Consul key/value data for key '{key}'");
                }

                actuals[i] = actual.ToUri();
            }

            return actuals;
        }
    }
}
