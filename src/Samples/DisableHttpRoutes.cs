using Bootstrapping.Configuration2;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.Testing.Samples
{
    public class DisableHttpRoutes
    {
        public void Go()
        {
            // SAMPLE: NoHttpRoutesInWebHoster
            var host = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseJasper(x => x.HttpRoutes.Enabled = false)
                .Start();

            // ENDSAMPLE
        }
    }

    // SAMPLE: NoHttpRoutesApp
    public class NoHttpRoutesApp : JasperRegistry
    {
        public NoHttpRoutesApp()
        {
            HttpRoutes.Enabled = false;
        }
    }
    // ENDSAMPLE
}
