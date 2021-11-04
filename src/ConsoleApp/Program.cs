// SAMPLE: QuickStartConsoleMain

using System.Threading.Tasks;
using Jasper;

namespace MyApp
{
    internal class Program
    {
        // You may need to enable C# 7.1 or higher for your project
        private static Task<int> Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppOptions
            // until the executable is stopped
            return JasperHost.Run<MyAppOptions>(args);

            // The code above is shorthand for the following:
            /*
            return Host
                .CreateDefaultBuilder()
                .UseJasper<MyAppOptions>()
                .RunOaktonCommands(args);
            */
        }
    }
}
// ENDSAMPLE
