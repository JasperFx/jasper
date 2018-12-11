using System.Threading.Tasks;
using Jasper.Messaging;

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

            // Enqueue a message locally non-durably
            await bus.EnqueueLightweight(new Message1());

            // Enqueue a message locally & durably
            await bus.EnqueueDurably(new Message1());
        }

        // ENDSAMPLE
    }
}
