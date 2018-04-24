using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging;
using Microsoft.Extensions.DependencyInjection;
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
}
