using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.Hosting;

namespace JasperService
{
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return CreateHostBuilder().RunJasper(args);
        }

        public static IHostBuilder CreateHostBuilder() =>
            Host
            .CreateDefaultBuilder()
            .UseJasper<JasperConfig>();
    
    }


}