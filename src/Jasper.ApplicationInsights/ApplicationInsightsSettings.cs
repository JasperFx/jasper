using System;
using Jasper.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Jasper.ApplicationInsights
{
    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; } = "Jasper";
    }

    public static class ApplicationInsightSettingsExtensions
    {
        /// <summary>
        /// If you already know your ApplicationInsights InstrumentationKey, use
        /// this to set it for the Jasper integration
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="instrumentationKey"></param>
        public static void ApplicationInsightsKeyIs(this JasperSettings settings, string instrumentationKey)
        {
            settings.Alter<ApplicationInsightsSettings>(x => x.InstrumentationKey = instrumentationKey);
        }

        /// <summary>
        /// Configure the Appication Insights instrumentation key value from
        /// the application's IConfiguration
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="valueSource"></param>
        public static void ApplicationInsightsKeyIs(this JasperSettings settings, Func<IConfiguration, string> valueSource)
        {
        settings.Alter<ApplicationInsightsSettings>((c, s) =>
        {
            s.InstrumentationKey = valueSource(c.Configuration);
        });
        }
    }
}
