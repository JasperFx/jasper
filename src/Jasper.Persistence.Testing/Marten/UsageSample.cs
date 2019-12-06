using Jasper.Persistence.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.Testing.Marten
{
    // SAMPLE: AppWithMarten
    public class AppWithMarten : JasperOptions
    {
        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // At the simplest, you would just need to tell Marten
            // the connection string to the application database
            Extensions.UseMarten(config.GetConnectionString("marten"));
        }
    }

    // ENDSAMPLE
}
