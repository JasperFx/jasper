using System.Threading.Tasks;
using Jasper;
using Jasper.Transports.Tcp;
using Microsoft.Extensions.Hosting;
using Oakton;

namespace Publisher
{
    internal class Program
    {
        public static Task<int> Main(string[] args)
        {
            return Host
                .CreateDefaultBuilder()
                .UseJasper(opts =>
                {
                    opts.ListenAtPort(2211);
                })
                .RunOaktonCommands(args);
        }
    }



}
