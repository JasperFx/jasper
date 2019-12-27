using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Testing.Samples
{
    // SAMPLE: JasperAppWithServices
    public class JasperAppWithServices : JasperOptions
    {
        public JasperAppWithServices()
        {
            // Add service registrations with the ASP.Net Core
            // DI abstractions
            Services.AddLogging();

            // or mix and match with StructureMap style
            // registrations
            Services.For(typeof(ILogger)).Use(typeof(Logger<>));
        }
    }
    // ENDSAMPLE


    public interface IThirdPartyService
    {
    }

    public class StubThirdPartyService : IThirdPartyService
    {
    }


}
