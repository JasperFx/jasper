using System.Threading.Tasks;
using Jasper.Messaging;
using TestMessages;

namespace Jasper.Testing.Samples
{
    public class EnqueueSamples
    {
        // SAMPLE: enqueue-locally
        public static async Task enqueue_locally(IMessageContext bus)
        {
            // Enqueue a message to the local, loopback transport
            // using the default worker queue & durability rules
            // for the message type
            await bus.Enqueue(new Message1());

        }

        // ENDSAMPLE
    }
}
