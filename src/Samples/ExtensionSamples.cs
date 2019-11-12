using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Jasper.Testing.Samples
{
    // SAMPLE: SampleExtension
    public class SampleExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            // Add service registrations
            options.Services.AddTransient<IFoo, Foo>();



            // Alter settings within the application
            options.Advanced.JsonSerialization
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
