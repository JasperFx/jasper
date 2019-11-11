using Jasper;
using Jasper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Testing.Samples
{
    public class Bootstrapping
    {
        public static void Go(string[] args)
        {
            // SAMPLE: Bootstrapping-Basic
            using (var host = JasperHost.Basic())
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic2
            using (var host = Host.CreateDefaultBuilder()
                .UseJasper()
                .Start())
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic3
            using (var runtime = JasperHost.For(_ =>
            {
                _.Transports.LightweightListenerAt(2001);
            }))
            {
                // do stuff
            }

            // ENDSAMPLE
        }
    }

    // SAMPLE: Bootstrapping-CustomJasperExtension
    public class CustomJasperExtension : IJasperExtension
    {
        public int Threshold { get; set; } = 10;

        public void Configure(JasperRegistry registry)
        {
            // apply alterations
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithExtensions
    public class AppWithExtensions : JasperRegistry
    {
        public AppWithExtensions()
        {
            // as is
            Include<CustomJasperExtension>();

            // or
            Include(new CustomJasperExtension());

            // or use the extension with customizations
            Include<CustomJasperExtension>(_ => { _.Threshold = 20; });
        }
    }
    // ENDSAMPLE


    public interface ISecurityService
    {
    }

    public class MySecurityService : ISecurityService
    {
    }

    // SAMPLE: Bootstrapping-ServiceRegistrations
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            // Adding services to the underlying container via
            // the ASP.Net Core DI abstractions
            Services.AddTransient<ISecurityService, MySecurityService>();

            // or via idiomatic StructureMap
            Services.For<ISecurityService>().Use<MySecurityService>();
        }
    }

    // ENDSAMPLE
}

namespace Bootstrapping.Configuration
{
    public class MyJasperApp : JasperRegistry
    {

    }
}

namespace Bootstrapping.Configuration2
{
    // SAMPLE: CustomJasperRegistry
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            ServiceName = "My Jasper App";

            Transports.DurableListenerAt(2111);
        }
    }
    // ENDSAMPLE

    public static class Program
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-with-custom-JasperRegistry
            using (var runtime = JasperHost.For<MyJasperApp>())
            {
                // do stuff
            }

            // or

            using (var runtime = JasperHost.For(new MyJasperApp()))
            {
                // do stuff
            }

            // ENDSAMPLE
        }
    }

    // SAMPLE: CustomServiceRegistry
    public class CustomServiceRegistry : JasperRegistry
    {
        public CustomServiceRegistry()
        {
            // The derived default would be "CustomService"
            ServiceName = "My Custom Service";
        }
    }
    // ENDSAMPLE


    public class Samples
    {
        public void using_web_host_builder()
        {
            // SAMPLE: aspnetcore-idiomatic-option-configuration
            var builder = Host.CreateDefaultBuilder()
                // Overwrite the environment name
                .UseEnvironment("Development")

                // Override Jasper settings
                .UseJasper(x =>
                {
                    x.ServiceName = "MyApplicationService";
                });

            // ENDSAMPLE
        }
    }


    public class Startup
    {
        public void Configure(JasperOptions options, IConfiguration configuration)
        {

        }
    }
}
