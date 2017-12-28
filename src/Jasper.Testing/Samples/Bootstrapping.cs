using System.IO;
using System.Threading.Tasks;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper;
using Jasper.Configuration;
using Jasper.Testing.Samples;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Testing.Samples
{
    public class Bootstrapping
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-Basic
            using (var runtime = JasperRuntime.Basic())
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic2
            using (var runtime = JasperRuntime.For(new JasperRegistry()))
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic3
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Transports.ListenForMessagesFrom("tcp://localhost:2001");
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
        public void Configure(JasperRegistry registry)
        {
            // apply alterations
        }

        public int Threshold { get; set; } = 10;
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
            Include<CustomJasperExtension>(_ =>
            {
                _.Threshold = 20;
            });
        }
    }
    // ENDSAMPLE


    public interface ISecurityService
    {

    }

    public class MySecurityService : ISecurityService{}

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
        public MyJasperApp()
        {
            // Set up the application configuration
            Configuration
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json");


        }
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

            Configuration.AddEnvironmentVariables();

            Transports.ListenForMessagesFrom("durable://localhost:2111/incoming");
        }
    }
    // ENDSAMPLE

    public static class Program
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-with-custom-JasperRegistry
            using (var runtime = JasperRuntime.For<MyJasperApp>())
            {
                // do stuff
            }

            // or

            using (var runtime = JasperRuntime.For(new MyJasperApp()))
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

    // SAMPLE: EnvironmentNameRegistry
    public class EnvironmentNameRegistry : JasperRegistry
    {
        public EnvironmentNameRegistry()
        {
            // By default, this is delegating to ASP.Net Core
            if (EnvironmentName == "Development")
            {
                Features.Include<DiagnosticServer>();
            }

            // Override the Environment
            EnvironmentName = "Production";

            // which is just a shortcut for:
            Http.UseEnvironment("Production");

        }
    }
    // ENDSAMPLE

    public class DiagnosticServer : IFeature
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<ServiceRegistry> Bootstrap(JasperRegistry registry)
        {
            throw new System.NotImplementedException();
        }

        public Task Activate(JasperRuntime runtime, GenerationRules generation)
        {
            throw new System.NotImplementedException();
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
