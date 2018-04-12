using System;
using Jasper;
using Jasper.CommandLine;
using Jasper.Http;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.AspNetCore.Hosting;
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
            Hosting.UseUrls("http://localhost:5004");
            Transports.Http.EnableListening(true);
            Subscribe.At("http://loadbalancer/messages");
            Subscribe.ToAllMessages();

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
