using System;
using System.Threading.Tasks;
using Jasper.Bus;

namespace Jasper.Consul
{
    public class ConsulUriLookup : IUriLookup
    {
        public string Protocol { get; } = "consul";
        public Task<Uri[]> Lookup(Uri[] originals)
        {
            throw new NotImplementedException();
        }
    }
}
