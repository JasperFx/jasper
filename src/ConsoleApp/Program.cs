// SAMPLE: QuickStartConsoleMain
using Jasper;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppRegistry
            // until the executable is stopped
            JasperAgent.Run<MyAppRegistry>();
        }
    }
}
// ENDSAMPLE
