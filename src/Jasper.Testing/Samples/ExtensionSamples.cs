using Jasper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Jasper.Testing.Samples
{
    // SAMPLE: SampleExtension
    public class SampleExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            // Add service registrations
            registry.Services.AddTransient<IFoo, Foo>();

            // Alter settings within the application
            registry.Settings.Alter<JasperOptions>(_ =>
            {
                _.JsonSerialization.TypeNameHandling = TypeNameHandling.All;
            });
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

    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
    }
}
