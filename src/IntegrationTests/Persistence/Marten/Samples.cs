using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Marten.Tests
{
    // SAMPLE: AppUsingMartenMessagePersistence
    public class AppUsingMartenMessagePersistence : JasperRegistry
    {
        public AppUsingMartenMessagePersistence()
        {
            // Use this line to activate the Marten-backed
            // message persistence for durable, store and forward
            // messaging
            Include<MartenBackedPersistence>();

            // "config" is the ASP.Net Core IConfiguration for the application
            // "options" is the Marten StoreOptions configuration object
            Settings.ConfigureMarten((context, options) =>
            {
                options.Connection(context.Configuration["marten_database"]);

                // Other Marten configuration
            });

            // Use a "durable" TCP listener at port
            // 2222 where the incoming messages will be
            // persisted with Marten upon receipt and
            // deleted only when the message is successfully
            // processed
            Transports.DurableListenerAt(2222);
        }
    }

    // ENDSAMPLE
}
