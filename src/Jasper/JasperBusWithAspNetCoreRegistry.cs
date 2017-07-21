using System;
using Jasper.Bus;
using Jasper.Http;
using Microsoft.AspNetCore.Hosting;

namespace Jasper
{
    [Obsolete("Going to make this be unnecessary")]
    public class JasperBusWithAspNetCoreRegistry : JasperRegistry
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
