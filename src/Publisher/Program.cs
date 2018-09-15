using Baseline;
using Jasper;
using Jasper.CommandLine;
using Jasper.Messaging.Transports.Configuration;
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

            // Assume that all concrete types in your application
            // in your application assembly that implement a marker
            // interface are published by the application
            // NOTE: IPublished is just an example and does not exist in Jasper
            Publish.MessagesMatching(x => x.CanBeCastTo<IPublished>());

            // This would be the Uri to the load balancer
            Subscribe.At("tcp://localhost:2211");
            Subscribe.ToAllMessages();
        }
    }
    // ENDSAMPLE

    public interface IPublished{}

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
