using System;
using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.Hosting;

namespace EFPlusSqlServerConsole
{
    class Program
    {
        public static Task<int> Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)

                .UseJasper<JasperConfig>()
                .RunJasper(args);
        }
    }
}
