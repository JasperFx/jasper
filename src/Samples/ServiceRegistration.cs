using Jasper.Messaging.Tracking;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Testing.Samples
{
    // SAMPLE: JasperAppWithServices
    public class JasperAppWithServices : JasperRegistry
    {
        public JasperAppWithServices()
        {
            // Add service registrations with the ASP.Net Core
            // DI abstractions
            Services.AddLogging();
            Services.AddSingleton(new MessageTracker());

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

    public class GetAtTheContainer
    {
        // SAMPLE: GetAtTheContainer
        public void retrieve_the_container(JasperRuntime runtime, IHost host)
        {
            // The root Lamar IoC container hangs directly
            // off the JasperRuntime if you're bootstrapping
            // the idiomatic Jasper way
            var container = runtime.Container;

            // Or if you are using ASP.Net Core bootstrapping,
            // the IHost.Services is actually the root
            // Lamar container
            var container2 = (IContainer) host.Services;
        }

        // ENDSAMPLE
    }
}
