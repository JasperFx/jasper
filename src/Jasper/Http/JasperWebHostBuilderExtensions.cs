using System;
using Jasper.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public static class JasperWebHostBuilderExtensions
    {

        public static IApplicationBuilder AddJasper(this IApplicationBuilder app)
        {
            throw new NotImplementedException();
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder)
        {
            throw new NotImplementedException();
        }

        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder) where T : JasperRegistry, new()
        {
            return builder.UseJasper(new T());
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            // TODO -- work over this entire thing

            // TODO -- right now this is assuming that it's registered last, but what if it's not?

            var runtime = JasperRuntime.For(registry);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(runtime);
                JasperStartup.Register(runtime.Container, services, null);
            });

            // TODO -- configure JasperHttp middleware if it exists

            return builder;
        }

        // TODO -- an option for basic Jasper Http
    }

}
