using System.IO;
using System.Linq;
using Lamar;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Util
{
    public class Thing
    {
        public ILogger<Thing> Logger { get; }

        public Thing(ILogger<Thing> logger)
        {
            Logger = logger;
        }
    }

    public class can_resolve_loggers_and_options
    {
        [Fact]
        public void with_aspnet_core()
        {
            var builder = JasperHost.CreateDefaultBuilder()

                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                })
                .UseJasper();

            var host = builder.Build();
            var services = host.Services;

            var options = services.GetService<IOptions<LoggerFilterOptions>>();
            var logging = options.Value;



            var logger = services.GetRequiredService<ILogger<Thing>>();


            logger.ShouldNotBeNull();


        }


    }
}
