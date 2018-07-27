using Jasper;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Subscriptions;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Persistence.Marten
{
    // SAMPLE: AppWithMartenBackedSubscriptions
    public class AppWithMartenBackedSubscriptions : JasperRegistry
    {
        public AppWithMartenBackedSubscriptions()
        {
            // Use the Include() method so that Jasper can
            // get the order of precedence right between
            // an extension and the application settings
            Include<MartenBackedSubscriptions>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppUsingMartenSubscriptions
    public class AppUsingMartenSubscriptions : JasperRegistry
    {
        public AppUsingMartenSubscriptions()
        {
            Include<MartenBackedSubscriptions>();

            Settings.Alter<MartenSubscriptionSettings>((context, settings) =>
            {
                settings.StoreOptions.Connection(context.Configuration.GetConnectionString("subscriptions"));
                settings.StoreOptions.DatabaseSchemaName = "subscriptions";
            });
        }
    }
    // ENDSAMPLE


    public class GreenMessage
    {
    }

    public class BlueMessage
    {
    }

    public class RedMessage
    {
    }

    public class OrangeMessage
    {
    }
}
