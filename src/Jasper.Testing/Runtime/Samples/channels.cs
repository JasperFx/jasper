using System.Threading.Tasks;
using TestMessages;

namespace Jasper.Testing.Runtime.Samples
{





    // SAMPLE: sending-messages-for-static-routing
    public class SendingExample
    {
        public async Task SendPingsAndPongs(IMessageContext bus)
        {
            // Publish a message
            await bus.Send(new PingMessage());
        }
    }
    // ENDSAMPLE


}
