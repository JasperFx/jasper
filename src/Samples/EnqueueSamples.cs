using System.Threading.Tasks;
using TestMessages;

namespace Jasper.Testing.Samples
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
