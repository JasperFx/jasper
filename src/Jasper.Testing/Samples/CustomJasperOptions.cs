using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Tracking;
using LamarCodeGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Testing.Samples
{

    public class SampleProgram1
    {
        // SAMPLE: UseJasperWithInlineOptionsConfiguration
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()

                // This adds Jasper with inline configuration
                // of JasperOptions
                .UseJasper(opts =>
                {
                    opts.Extensions.UseMessageTrackingTestingSupport();
                });
        // ENDSAMPLE

    }

    public class SampleProgram2
    {
        // SAMPLE: UseJasperWithInlineOptionsConfigurationAndHosting
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()

                // This adds Jasper with inline configuration
                // of JasperOptions
                .UseJasper((context, opts) =>
                {
                    // This is an example usage of the application's
                    // IConfiguration inside of Jasper bootstrapping
                    var port = context.Configuration.GetValue<int>("ListenerPort");
                    opts.Endpoints.ListenAtPort(port);

                    // If we're running in development mode and you don't
                    // want to worry about having all the external messaging
                    // dependencies up and running, stub them out
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        opts.Endpoints.StubAllExternallyOutgoingEndpoints();
                        opts.Extensions.UseMessageTrackingTestingSupport();
                    }
                });
        // ENDSAMPLE

    }





    // SAMPLE: CustomJasperOptions
    public class CustomJasperOptions : JasperOptions
    {
        public CustomJasperOptions()
        {
            // Static configuration can go here


            ServiceName = "CustomApp";
        }

        // It's optional, but especially if you're using Jasper for messaging between
        // applications or using the command/persistence, you'll almost certainly need
        // to use application configuration with Jasper
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var port = config.GetValue<int>("ListenerPort");
            Endpoints.ListenAtPort(port);

            // If we're running in development mode and you don't
            // want to worry about having all the external messaging
            // dependencies up and running, stub them out
            if (hosting.IsDevelopment())
            {
                Extensions.UseMessageTrackingTestingSupport();
                Endpoints.StubAllExternallyOutgoingEndpoints();
            }
        }
    }
    // ENDSAMPLE

    // SAMPLE: JasperOptionsWithEverything
    public class JasperOptionsWithEverything : JasperOptions
    {
        public JasperOptionsWithEverything()
        {
            // This is strictly for logging and diagnostics identification
            ServiceName = "MyService";

            // Extensions lets you apply or query Jasper extensions
            Extensions.UseMessageTrackingTestingSupport();

            // As the name implies, these are seldom used options
            // that fine-tune Jasper behavior
            Advanced.MaximumEnvelopeRetryStorage = 5000;

            // Idiomatic Lamar service registrations to be applied to the
            // application's underlying IoC container
            Services.For<IClock>().Use<Clock>();

            // Fine-tune how message handler types are discovered
            // and built. Also allows you to add middleware policies
            Handlers.DisableConventionalDiscovery();
        }

        // This method is an optional override for additional
        // Jasper configuration that is dependent upon either the
        // hosting environment name or the application configuration
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Endpoints controls everything about where and how
            // Jasper receives incoming or sends outgoing messages
            // For the moment, this also gives you access to fine-tuning
            // the in-process worker queues for the local transport

            var incomingPort = config.GetValue<int>("incoming_port");
            Endpoints.ListenAtPort(incomingPort);

            var outgoingPort = config.GetValue<int>("outgoing_port");
            Endpoints.PublishAllMessages().ToPort(outgoingPort);
            Endpoints.LocalQueue("worker1").Sequential();
        }
    }
    // ENDSAMPLE


    public class SampleProgram3
    {
        // SAMPLE: UseJasperWithCustomJasperOptions
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .UseJasper<CustomJasperOptions>();
        // ENDSAMPLE

    }
}
