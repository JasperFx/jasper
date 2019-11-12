using Jasper.Persistence.Marten;
using Microsoft.Extensions.Configuration;

namespace Jasper.Persistence.Testing.Marten
{
    // SAMPLE: AppWithMarten
    public class AppWithMarten : JasperOptions
    {
        public AppWithMarten()
        {
            // StoreOptions is a Marten object that fulfills the same
            // role as JasperOptions
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
