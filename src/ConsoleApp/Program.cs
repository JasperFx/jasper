// SAMPLE: QuickStartConsoleMain

using Jasper.CommandLine;

namespace MyApp
{
    class Program
    {
        static int Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppRegistry
            // until the executable is stopped
            return JasperAgent.Run<MyAppRegistry>(args);
        }
    }
}
// ENDSAMPLE


