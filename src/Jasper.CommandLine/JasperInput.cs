using System;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Oakton;

namespace Jasper.CommandLine
{
    // SAMPLE: JasperInput
    public class JasperInput
    {
        [IgnoreOnCommandLine]
        public JasperRegistry Registry { get; set; }

        [Description("Use to override the ASP.Net Environment name")]
        public string EnvironmentFlag { get; set; }

        [Description("Write out much more information at startup and enables console logging")]
        public bool VerboseFlag { get; set; }

        [Description("Override the log level")]
        public LogLevel? LogLevelFlag { get; set; }

        public JasperRuntime BuildRuntime()
        {
            if (LogLevelFlag.HasValue)
            {
                Registry.ConfigureLogging(x => x.SetMinimumLevel(LogLevelFlag.Value));
            }

            if (VerboseFlag)
            {
                Console.WriteLine("Verbose flag is on.");

                Registry.ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Debug);

                    x.AddConsole();
                    x.AddDebug();
                });
            }

            if (EnvironmentFlag.IsNotEmpty())
            {
                Registry.UseEnvironment(EnvironmentFlag);
            }

            return JasperRuntime.For(Registry);
        }
    }
    // ENDSAMPLE
}
