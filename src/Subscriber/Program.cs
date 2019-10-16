using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using TestMessages;

namespace Subscriber
{
    internal class Program
    {
        private static Task<int> Main(string[] args)
        {

            return JasperHost.Run<SubscriberApp>(args);
        }
    }

    public class SubscriberApp : JasperRegistry
    {
        public SubscriberApp()
        {
            Transports.LightweightListenerAt(22222);
        }
    }


    public class NewUserHandler
    {
        public void Handle(NewUser newGuy)
        {
        }

        public void Handle(DeleteUser deleted)
        {
        }
    }
}
