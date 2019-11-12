using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;
using TestMessages;

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
            Transports.LightweightListenerAt(2211);

            // 100% Optional for diagnostics
            Publish.Message<NewUser>();
            Publish.Message<DeleteUser>();
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
