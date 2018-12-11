using Jasper;
using Jasper.ApplicationInsights;

namespace LoadTestDebugger
{
    // SAMPLE: AppInsightsUsingApp
    public class AppInsightsUsingApp : JasperRegistry
    {
        public AppInsightsUsingApp()
        {
            // TODO -- use the new helper syntax here instead
            Settings.Alter<ApplicationInsightsSettings>((context, settings) =>
            {
                // Assuming that you're putting the InstrumentationKey in your application's
                // configuration under the key "appinsightsKey"
                settings.InstrumentationKey = context.Configuration["appinsightsKey"];
            });
        }
    }

    // ENDSAMPLE
}
