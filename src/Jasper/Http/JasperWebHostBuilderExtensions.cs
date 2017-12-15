using System;
using Baseline;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public static class JasperWebHostBuilderExtensions
    {
        private const string JasperRouterKey = "JasperRouter";
        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";

        internal static void StoreRouter(this IApplicationBuilder builder, Router router)
        {
            builder.Properties.Add(JasperRouterKey, router);
        }

        internal static void MarkJasperHasBeenApplied(this IApplicationBuilder builder)
        {
            if (!builder.Properties.ContainsKey(JasperHasBeenApplied))
            {
                builder.Properties.Add(JasperHasBeenApplied, true);
            }
        }

        internal static bool HasJasperBeenApplied(this IApplicationBuilder builder)
        {
            return builder.Properties.ContainsKey(JasperHasBeenApplied);
        }

        public static IApplicationBuilder AddJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
            {
                throw new InvalidOperationException("Jasper has already been applied to this web application");
            }

            var router = app.Properties.ContainsKey(JasperRouterKey)
                ? app.Properties[JasperRouterKey].As<Router>()
                : app.ApplicationServices.GetRequiredService<Router>();

            app.MarkJasperHasBeenApplied();

            return app.Use(next => c => router.Invoke(c, next));
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder)
        {
            return builder.UseJasper(new JasperRegistry());
        }

        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder) where T : JasperRegistry, new()
        {
            return builder.UseJasper(new T());
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            builder.UseSetting(WebHostDefaults.ApplicationKey, registry.ApplicationAssembly.FullName);

            registry.Features.For<AspNetCoreFeature>().BootstrappedWithinAspNetCore = true;
            var runtime = JasperRuntime.For(registry);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(runtime);

                JasperStartup.Register(runtime.Container, services, registry.Features.For<AspNetCoreFeature>().Routes.Router);
            });


            return builder;
        }
    }

}
