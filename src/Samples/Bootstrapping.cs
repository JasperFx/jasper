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
                _.Endpoints.ListenAtPort(2001);
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

        public void Configure(JasperOptions options)
        {
            // apply alterations
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithExtensions
    public class AppWithExtensions : JasperOptions
    {
        public AppWithExtensions()
        {
            // as is
            Extensions.Include<CustomJasperExtension>();

            // or
            Extensions.Include(new CustomJasperExtension());

            // or use the extension with customizations
            Extensions.Include<CustomJasperExtension>(_ => { _.Threshold = 20; });
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
    public class MyJasperApp : JasperOptions
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
    public class MyJasperApp : JasperOptions
    {

    }
}

namespace Bootstrapping.Configuration2
{
    // SAMPLE: CustomJasperOptions
    public class MyJasperApp : JasperOptions
    {
        public MyJasperApp()
        {
            ServiceName = "My Jasper App";

            Endpoints.ListenAtPort(2111).Durable();
        }
    }
    // ENDSAMPLE

    public static class Program
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-with-custom-JasperOptions
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

    // SAMPLE: CustomServiceOptions
    public class CustomServiceOptions : JasperOptions
    {
        public CustomServiceOptions()
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
