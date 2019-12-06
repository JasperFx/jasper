using Jasper.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    // SAMPLE: SagaApp-with-Marten
    public class MartenSagaApp : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Assuming that your Postgresql database connection string
            // is in configuration as "marten"
            Extensions.UseMarten(config.GetConnectionString("marten"));
        }
    }

    // ENDSAMPLE
}
