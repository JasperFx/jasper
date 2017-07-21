using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Configuration;
using Jasper.Consul.Internal;

[assembly:JasperModule(typeof(ConsulExtension))]

namespace Jasper.Consul.Internal
{
    public class ConsulExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.For<IUriLookup>().Add<ConsulUriLookup>();
            registry.Services.For<IConsulGateway>().Add<ConsulGateway>();
        }
    }
}
