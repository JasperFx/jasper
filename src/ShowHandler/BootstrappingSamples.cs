using Jasper;
using Jasper.Bus.Transports.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ShowHandler
{
    public static class BootstrappingSamples
    {
        public static void AspNetCore()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls("http://localhost:3003");

            using (var host = builder.Build())
            {
                // run your application
            }
        }

        public static void Jasper()
        {
            using (var runtime = JasperRuntime.For<MyJasperApp>())
            {
                // run your application
            }
        }


    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Configure stuff in your ASP.Net Core application
        }
    }


    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            Transports.LightweightListenerAt(2222);
        }
    }

}
