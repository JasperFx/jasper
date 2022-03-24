using Jasper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Samples
{
    #region sample_SampleExtension
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
    #endregion


    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
    }
}
