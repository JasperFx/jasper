using System;
using Oakton;

namespace Jasper.CommandLine
{
    // SAMPLE: JasperInput
    public class JasperInput
    {
        [IgnoreOnCommandLine]
        public JasperRegistry Registry { get; set; }

        [Description("Use to override the ASP.Net Environment name")]
        public string EnvironmentFlag
        {
            set => JasperEnvironment.Name = value;

        }

        [Description("Write out much more information at startup and enables console logging")]
        public bool VerboseFlag { get; set; }

        public JasperRuntime BuildRuntime()
        {
            if (VerboseFlag)
            {
                Console.WriteLine("Verbose flag is on.");
                Registry.Logging.UseConsoleLogging = true;
                Registry.Logging.Verbose = true;
            }

            return JasperRuntime.For(Registry);
        }
    }
    // ENDSAMPLE
}
