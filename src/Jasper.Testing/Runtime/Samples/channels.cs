using System.Threading.Tasks;
using TestingSupport.Compliance;
using TestMessages;

namespace Jasper.Testing.Runtime.Samples
{





    // SAMPLE: sending-messages-for-static-routing
    public class SendingExample
    {
        public async Task SendPingsAndPongs(IExecutionContext bus)
        {
            // Publish a message
            await bus.SendAsync(new PingMessage());
        }
    }
    // ENDSAMPLE


}
