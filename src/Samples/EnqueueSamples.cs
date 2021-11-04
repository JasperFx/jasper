using System.Threading.Tasks;
using Jasper;
using TestMessages;

namespace Samples
{
    public class EnqueueSamples
    {
        // SAMPLE: enqueue-locally
        public static async Task enqueue_locally(ICommandBus bus)
        {
            // Enqueue a message to the local worker queues
            await bus.Enqueue(new Message1());

        }

        // ENDSAMPLE
    }
}
