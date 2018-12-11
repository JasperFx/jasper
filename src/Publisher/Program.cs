using Jasper;
using Jasper.CommandLine;
using TestMessages;

namespace Publisher
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return JasperAgent.Run<PublisherApp>(args);
        }
    }

    // SAMPLE: PublisherApp
    public class PublisherApp : JasperRegistry
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
