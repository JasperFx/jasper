using System;
using System.Reflection;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Oakton;

namespace Jasper.CommandLine
{
    // SAMPLE: JasperInput
    public class JasperInput
    {

        [Description("Use to override the ASP.Net Environment name")]
        public string EnvironmentFlag { get; set; }

        [Description("Write out much more information at startup and enables console logging")]
        public bool VerboseFlag { get; set; }

        [Description("Override the log level")]
        public LogLevel? LogLevelFlag { get; set; }

        [IgnoreOnCommandLine] public IWebHostBuilder WebHostBuilder { get; set; }

        [IgnoreOnCommandLine] public Assembly ApplicationAssembly { get; set; }

        public IJasperHost BuildHost(StartMode mode)
        {
            // SAMPLE: what-the-cli-is-doing

            // The --log-level flag value overrides your application's
            // LogLevel
            if (LogLevelFlag.HasValue) WebHostBuilder.ConfigureLogging(x => x.SetMinimumLevel(LogLevelFlag.Value));

            if (VerboseFlag)
            {
                Console.WriteLine("Verbose flag is on.");

                // The --verbose flag adds console and
                // debug logging, as well as setting
                // the minimum logging level down to debug
                WebHostBuilder.ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);

                    x.AddConsole();
                    x.AddDebug();
                });
            }

            // The --environment flag is used to set the environment
            // property on the IHostedEnvironment within your system
            if (EnvironmentFlag.IsNotEmpty()) WebHostBuilder.UseEnvironment(EnvironmentFlag);
            // ENDSAMPLE

            return mode == StartMode.Full ? WebHostBuilder.StartJasper() : new JasperRuntime(WebHostBuilder.Build());
        }
    }

    // ENDSAMPLE
}
