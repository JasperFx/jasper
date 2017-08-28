using System;
using Oakton;

namespace Jasper.CommandLine
{
    public class JasperInput
    {
        internal JasperRegistry Registry { get; set; }

        [Description("Use to override the ASP.Net Environment name")]
        public string EnvironmentFlag
        {
            set => JasperEnvironment.Name = value;

        }

        [Description("Write out much more information at startup and enables console logging")]
        public bool VerboseFlag { get; set; }

        internal JasperRuntime BuildRuntime()
        {
            if (VerboseFlag)
            {
                Console.WriteLine("Verbose flag is on.");
                Registry.Logging.UseConsoleLogging = true;
            }

            return JasperRuntime.For(Registry);
        }
    }
}