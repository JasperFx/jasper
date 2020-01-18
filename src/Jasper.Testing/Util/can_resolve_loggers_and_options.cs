using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Util
{
    public class Thing
    {
        public Thing(ILogger<Thing> logger)
        {
            Logger = logger;
        }

        public ILogger<Thing> Logger { get; }
    }

    public class can_resolve_loggers_and_options
    {
        [Fact]
        public void with_aspnet_core()
        {
            var builder = Host.CreateDefaultBuilder()
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
