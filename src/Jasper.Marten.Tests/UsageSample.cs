using Marten;
using Microsoft.Extensions.Configuration;

namespace Jasper.Marten.Tests
{
    // SAMPLE: AppWithMarten
    public class AppWithMarten : JasperRegistry
    {
        public AppWithMarten()
        {
            // StoreOptions is a Marten object that fulfills the same
            // role as JasperRegistry
            Settings.Alter<StoreOptions>((config, marten) =>
            {
                // At the simplest, you would just need to tell Marten
                // the connection string to the application database
                marten.Connection(config.GetConnectionString("marten"));
            });
        }
    }
    // ENDSAMPLE
}
