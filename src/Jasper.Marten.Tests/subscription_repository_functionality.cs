using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Marten.Subscriptions;
using Jasper.Marten.Tests.Setup;
using Jasper.Util;
using Marten;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests
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

    public class OrangeMessage{}
}
