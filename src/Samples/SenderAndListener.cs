using Jasper;

namespace Samples
{
    // SAMPLE: SenderAndListener
    public class SenderAndListener : JasperOptions
    {
        public SenderAndListener()
        {
            // All messages get published via TCP
            // to port 5555 on the local box
            Endpoints
                .PublishAllMessages()
                .To("tcp://localhost:5555");


            // Listen for incoming messages at
            // port 6666
            Endpoints
                .ListenForMessagesFrom("tcp://localhost:6666");
        }
    }
    // ENDSAMPLE
}
