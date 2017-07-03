using System;
using Jasper.Bus;

namespace Jasper.Consul
{
    public class ConsulUriLookup : IUriLookup
    {
        public string Protocol { get; } = "consul";
        public Uri Lookup(Uri original)
        {
            throw new NotImplementedException();
        }
    }
}
