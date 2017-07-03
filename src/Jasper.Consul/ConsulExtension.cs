using Jasper;
using Jasper.Bus;
using Jasper.Configuration;
using Jasper.Consul;

[assembly:JasperModule(typeof(ConsulExtension))]

namespace Jasper.Consul
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
