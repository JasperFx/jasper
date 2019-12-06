using Jasper.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.Testing.Marten.Persistence.Sagas
{
    // SAMPLE: SagaApp-with-Marten
    public class MartenSagaApp : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // This example pulls the connection string to the underlying Postgresql
            // database from configuration
            Extensions.UseMarten(config.GetConnectionString("connectionString"));
        }
    }

    // ENDSAMPLE
}
