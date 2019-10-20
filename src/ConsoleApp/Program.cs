// SAMPLE: QuickStartConsoleMain

using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace MyApp
{
    internal class Program
    {
        // You may need to enable C# 7.1 or higher for your project
        private static Task<int> Main(string[] args)
        {
            // This bootstraps and runs the Jasper
            // application as defined by MyAppRegistry
            // until the executable is stopped
            return JasperHost.Run<MyAppRegistry>(args);

            // The code above is shorthand for the following:
            /*
            return Host
                .CreateDefaultBuilder()
                .UseJasper<MyAppRegistry>()
                .RunOaktonCommands(args);
            */
        }
    }
}
// ENDSAMPLE
