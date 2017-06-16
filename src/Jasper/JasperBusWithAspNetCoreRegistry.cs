using Jasper.Bus;
using Jasper.Http;
using Microsoft.AspNetCore.Hosting;

namespace Jasper
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
