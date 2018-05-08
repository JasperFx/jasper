using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging;
using Lamar;
using Microsoft.AspNetCore.Hosting;
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


    // SAMPLE: UsingEnvironmentName
    public class UsingEnvironmentName : JasperRegistry
    {
        public UsingEnvironmentName()
        {
            // Idiomatic Jasper way
            Settings.Configure(context =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    // If in development, I want to replace some kind
                    // of problematic 3rd party service wrapper with
                    // a nicely behaved stub
                    Services.AddSingleton<IThirdPartyService, StubThirdPartyService>();
                }
            });

            // ASP.Net Core idiomatic way
            Hosting.ConfigureServices((context, services) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    // If in development, I want to replace some kind
                    // of problematic 3rd party service wrapper with
                    // a nicely behaved stub
                    services.AddSingleton<IThirdPartyService, StubThirdPartyService>();
                }
            });
        }
    }
    // ENDSAMPLE

    public interface IThirdPartyService{}

    public class StubThirdPartyService : IThirdPartyService
    {

    }

    public class GetAtTheContainer
    {
        // SAMPLE: GetAtTheContainer
        public void retrieve_the_container(JasperRuntime runtime, IWebHost host)
        {
            // The root Lamar IoC container hangs directly
            // off the JasperRuntime if you're bootstrapping
            // the idiomatic Jasper way
            var container = runtime.Container;

            // Or if you are using ASP.Net Core bootstrapping,
            // the IWebHost.Services is actually the root
            // Lamar container
            var container2 = (IContainer)host.Services;
        }
        // ENDSAMPLE
    }
}
