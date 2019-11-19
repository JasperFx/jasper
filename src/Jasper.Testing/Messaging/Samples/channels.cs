using System;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Util;
using TestMessages;

namespace Jasper.Testing.Messaging.Samples
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
