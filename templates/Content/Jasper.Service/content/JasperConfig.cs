using Jasper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


namespace JasperService
{
    internal class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            // Any static configuration that does not depend
            // on the environment or configuration. Can be omitted.
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Additional Jasper configuration using the application's IHostEnvironment
            // and compiled IConfiguration

            // This method can be omitted
        }
    }

}