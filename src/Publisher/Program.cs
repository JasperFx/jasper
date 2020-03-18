using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace Publisher
{
    internal class Program
    {
        public static Task<int> Main(string[] args)
        {
            return Host
                .CreateDefaultBuilder()
                .UseJasper<PublisherApp>()
                .RunOaktonCommands(args);
        }
    }

    // SAMPLE: PublisherApp
    public class PublisherApp : JasperOptions
    {
        public PublisherApp()
        {
            Endpoints.ListenAtPort(2211);

        }
    }
    // ENDSAMPLE


}
