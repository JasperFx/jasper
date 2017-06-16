using Jasper.Http;
using JasperBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace JasperServer
{
    public class JasperBusWithAspNetCoreRegistry : JasperBusRegistry
    {
        public JasperBusWithAspNetCoreRegistry()
        {
            UseFeature<AspNetCoreFeature>();
        }

        public IWebHostBuilder WebHostBuilder => Feature<AspNetCoreFeature>().WebHostBuilder;

        public void UseStartup<T>() where T : class => WebHostBuilder.UseStartup<T>();

        public HostingConfiguration Hosting => Feature<AspNetCoreFeature>().Hosting;
    }


}
