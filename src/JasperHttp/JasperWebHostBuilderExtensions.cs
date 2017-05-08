using Jasper;
using JasperHttp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JasperHttp
{
    public static class JasperWebHostBuilderExtensions
    {
        public static IWebHostBuilder AddJasper<T>(this IWebHostBuilder builder) where T : JasperRegistry, new()
        {
            return builder.AddJasper(new T());
        }

        public static IWebHostBuilder AddJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            // TODO -- right now this is assuming that it's registered last, but what if it's not?

            var runtime = JasperRuntime.For(registry);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(runtime);
                JasperStartup.Register(runtime.Container, services);
            });

            // TODO -- configure JasperHttp middleware if it exists

            return builder;
        }

        // TODO -- an option for basic Jasper Http
    }

}
