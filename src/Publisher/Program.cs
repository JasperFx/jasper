using Jasper;
using Jasper.CommandLine;
using Jasper.Consul;
using TestMessages;

namespace Publisher
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            JasperAgent.Run<PublisherApp>(args);
        }
    }

    public class PublisherApp : JasperRegistry
    {
        public PublisherApp()
        {
            Include<ConsulBackedSubscriptions>();

            Transports.Lightweight.ListenOnPort(2211);

            // 100% Optional for diagnostics
            Publish.Message<NewUser>();
            Publish.Message<DeleteUser>();

            // This would be the Uri to the load balancer
            Subscribe.At("tcp://localhost:2211");
            Subscribe.ToAllMessages();
        }
    }

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
