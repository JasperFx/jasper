using System;
using Baseline.Dates;
using Jasper;
using Jasper.Tcp;
using Jasper.Tracking;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Samples
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
                    opts.ListenAtPort(port);

                    // If we're running in development mode and you don't
                    // want to worry about having all the external messaging
                    // dependencies up and running, stub them out
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        // This will "stub" out all configured external endpoints
                        opts.StubAllExternallyOutgoingEndpoints();

                        opts.Extensions.UseMessageTrackingTestingSupport();
                    }
                });
        // ENDSAMPLE

    }


}
