using System.Net;
using System.Net.Http;
using Baseline.Dates;
using Consul;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Jasper.Consul.Testing
{
    // SAMPLE: configuring-consul-in-jasperregistry
    public class AppUsingConsul : JasperRegistry
    {
        public AppUsingConsul()
        {
            // In this case, we're expecting an environment
            // variable called "Consul.Port"
            Configuration.AddEnvironmentVariables();

            Settings.Alter<ConsulSettings>(ConfigureConsul);
        }

        public void ConfigureConsul(WebHostBuilderContext context, ConsulSettings settings)
        {
            var config = context.Configuration;

            // Shorthand to use the default Consul setup, but with a different port
            // number retrieved from the application configuration
            settings.Port = config.GetValue<int>("Consul.Port");

            // Configure the underlying ConsulClientConfiguration object
            settings.Configure = _ =>
            {
                // This line is an equivalent to "settings.Port = #" as shown above
                _.Address = $"http://localhost:{config["Consul.Port"]}".ToUri();

                _.Token = config["Consul.Token"];
            };

            // ConsulDotNet allows you to further configure the HttpClient
            // that it uses internallly to talk to Consul
            settings.ClientOverride = httpClient =>
            {
                httpClient.Timeout = 1.Seconds();
            };

            // ConsulDotNet also allows you to configure the HttpClientHandler
            // that it uses internally to talk to Consul
            settings.HandlerOverride = httpClientHandler =>
            {
                httpClientHandler.AutomaticDecompression = DecompressionMethods.None;
            };
        }
    }
    // ENDSAMPLE
}
