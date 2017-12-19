using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Marten.Subscriptions;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Bus;
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

            Settings.Alter<MartenSubscriptionSettings>((config, settings) =>
            {
                settings.StoreOptions.Connection(config.GetConnectionString("subscriptions"));
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
