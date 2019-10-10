using Jasper;
using Jasper.ApplicationInsights;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Settings;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;

[assembly: JasperModule(typeof(ApplicationInsightsExtension))]

namespace Jasper.ApplicationInsights
{
    public class ApplicationInsightsExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<ApplicationInsightsSettings>();

            registry.Services.AddSingleton<IMetrics, ApplicationInsightsMetrics>();

            registry.Services.AddSingleton(s =>
            {
                var config = s.GetService<ApplicationInsightsSettings>();
                var client = new TelemetryClient {InstrumentationKey = config.InstrumentationKey};


                return client;
            });
        }
    }
}
