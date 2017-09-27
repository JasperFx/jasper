using Jasper.Bus.Transports.Configuration;
using Jasper.Configuration;
using Jasper.Testing.AspNetCoreIntegration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Jasper.Testing.Samples
{
    // SAMPLE: SampleExtension
    public class SampleExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            // Not sure *why* you'd do this, but you could
            registry.Configuration.AddJsonFile("someFile.json");


            // Add service registrations
            ServiceCollectionServiceExtensions.AddTransient<IFoo, Foo>(registry.Services);

            // Alter settings within the application
            registry.Settings.Alter<BusSettings>(_ =>
            {
                _.JsonSerialization.TypeNameHandling = TypeNameHandling.All;
            });

            // Register additional ASP.Net Core middleware,
            // but it'd probably be better to use an IStartupFilter
            // for ordering instead
            registry.Http.AddSomeMiddleware();
        }
    }
    // ENDSAMPLE

    public static class MiddlewareExtensions
    {
        public static IWebHostBuilder AddSomeMiddleware(this IWebHostBuilder builder)
        {
            return builder;
        }
    }
}
