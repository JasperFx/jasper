using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Jasper.Testing.Samples
{
    // SAMPLE: SampleExtension
    public class SampleExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            // Add service registrations
            registry.Services.AddTransient<IFoo, Foo>();



            // Alter settings within the application
            registry.Advanced.JsonSerialization
                .TypeNameHandling = TypeNameHandling.All;
        }
    }
    // ENDSAMPLE


    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
    }
}
