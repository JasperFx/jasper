using Jasper;
using Microsoft.AspNetCore.Hosting;
using StorytellerSpecs.Fixtures;

namespace MyApp
{

    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppRegistry : JasperRegistry
    {
        public MyAppRegistry()
        {
            Http.UseKestrel().UseUrls("http://localhost:3001");

            // TODO -- use the new syntax from GH-163 when it exists
            Channels.ListenForMessagesFrom("tcp://localhost:2222/incoming");



            Publishing.Message<Message1>();
            Publishing.Message<Message2>();
            Publishing.Message<Message3>();

            Subscriptions.At("tcp://server1:2222");
            Subscriptions.To<Message4>();
            Subscriptions.To<Message5>();
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
