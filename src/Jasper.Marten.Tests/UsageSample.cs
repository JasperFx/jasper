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
            Settings.ConfigureMarten((context, marten) =>
            {
                // At the simplest, you would just need to tell Marten
                // the connection string to the application database
                marten.Connection(context.Configuration.GetConnectionString("marten"));
            });
        }
    }

    // ENDSAMPLE
}
