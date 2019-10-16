using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MvcCoreHybrid
{
    // SAMPLE: MvcCoreHybrid.Program
    public class Program
    {
        // Return an int for a status code
        public static Task<int> Main(string[] args)
        {
            // Calling RunJasper() opts into Jasper's expansive
            // command line skeleton with diagnostics you probably
            // want
            return CreateWebHostBuilder(args).RunJasper(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()

                // Add Jasper with all its defaults
                .UseJasper();
    }
    // ENDSAMPLE



}
