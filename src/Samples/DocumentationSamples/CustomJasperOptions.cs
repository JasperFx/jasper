using Jasper;
using Jasper.Tcp;
using Jasper.Tracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DocumentationSamples
{

    public class SampleProgram1
    {
        #region sample_UseJasperWithInlineOptionsConfiguration
        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()

                // This adds Jasper with inline configuration
                // of JasperOptions
                .UseJasper(opts =>
                {
                    opts.Extensions.UseMessageTrackingTestingSupport();
                });
        #endregion

    }

    public class SampleProgram2
    {
        #region sample_UseJasperWithInlineOptionsConfigurationAndHosting
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
        #endregion

    }


}
