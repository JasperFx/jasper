using System;
using Jasper;
using Jasper.CommandLine;
using TestMessages;

namespace Subscriber
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<SubscriberApp>(args);
        }
    }

    public class SubscriberApp : JasperRegistry
    {
        public SubscriberApp()
        {
            Subscribe.At("http://loadbalancer/messages");
            Subscribe.ToAllMessages();
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
