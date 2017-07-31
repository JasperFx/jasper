using System;
using Baseline;
using Jasper.Http.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public static class JasperWebHostBuilderExtensions
    {
        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";

        public static void StoreRouter(this IApplicationBuilder builder, Router router)
        {
            builder.Properties.Add("JasperRouter", router);
        }

        public static void MarkJasperHasBeenApplied(this IApplicationBuilder builder)
        {
            if (!builder.Properties.ContainsKey(JasperHasBeenApplied))
            {
                builder.Properties.Add(JasperHasBeenApplied, true);
            }
        }

        public static bool HasJasperBeenApplied(this IApplicationBuilder builder)
        {
            return builder.Properties.ContainsKey(JasperHasBeenApplied);
        }

        public static IApplicationBuilder AddJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
            {
                throw new InvalidOperationException("Jasper has already been applied to this web application");
            }

            if (app.Properties.ContainsKey("JasperRouter"))
            {
                var router = app.Properties["JasperRouter"].As<Router>();

                app.MarkJasperHasBeenApplied();
                return app.Use(next => c => router.Invoke(c, next));
            }
            else
            {
                // PUll the router out of services?
                throw new NotImplementedException();
            }

            return app;
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
                JasperStartup.Register(runtime.Container, services, runtime.Get<RouteGraph>().Router);
            });

            // TODO -- configure JasperHttp middleware if it exists

            return builder;
        }

        // TODO -- an option for basic Jasper Http
    }

}
