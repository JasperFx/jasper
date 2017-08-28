// SAMPLE: QuickStartConsoleMain
using Jasper;
using Jasper.CommandLine;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppRegistry
            // until the executable is stopped
            JasperAgent.Run<MyAppRegistry>(args);
        }
    }
}
// ENDSAMPLE


