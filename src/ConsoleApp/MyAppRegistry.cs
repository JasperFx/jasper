using Jasper;
using Jasper.Http;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.AspNetCore.Hosting;
using StorytellerSpecs.Fixtures;

namespace MyApp
{

    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppRegistry : JasperHttpRegistry
    {
        public MyAppRegistry()
        {
            Http.UseKestrel().UseUrls("http://localhost:3001");

            Transports.LightweightListenerAt(2222);

            Publish.Message<Message1>();
            Publish.Message<Message2>();
            Publish.Message<Message3>();

            Subscribe.At("tcp://server1:2222");
            Subscribe.To<Message4>();
            Subscribe.To<Message5>();
        }
    }
    // ENDSAMPLE


    public class MessageHandler
    {
        public void Handle(Message1 msg){}
        public void Handle(Message2 msg){}
        public void Handle(Message3 msg){}
        public void Handle(Message4 msg){}
        public void Handle(Message5 msg){}
    }
}
