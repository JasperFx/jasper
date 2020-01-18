using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;
using TestMessages;

#if NETSTANDARD2_0
using Host = Microsoft.AspNetCore.WebHost;
#else
using Host = Microsoft.Extensions.Hosting.Host;
#endif

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

    public class UserHandler
    {
        public void Handle(UserCreated message)
        {
        }

        public void Handle(UserDeleted message)
        {
        }
    }
}
