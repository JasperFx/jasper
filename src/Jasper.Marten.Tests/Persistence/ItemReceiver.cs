using Jasper.Bus.Transports.Configuration;
using Jasper.Marten.Tests.Setup;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Marten.Tests.Persistence
{
    public class ItemReceiver : JasperRegistry
    {
        public ItemReceiver()
        {
            Include<MartenBackedPersistence>();

            Settings.Alter<StoreOptions>(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
            });

            Services.AddSingleton<Testing.Bus.Lightweight.MessageTracker>();

            Transports.DurableListenerAt(2345);
        }
    }
}
