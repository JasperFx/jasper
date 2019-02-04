using System;
using Jasper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace JasperHttp
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return CreateWebHostBuilder(args).RunJasper(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseJasper<JasperConfig>();
                
    }


}