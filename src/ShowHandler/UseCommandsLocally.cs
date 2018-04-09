using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;

namespace ShowHandler
{
    public class UseCommandsLocally
    {
        public async Task run_locally(IMessageContext context)
        {
            var command = new CreateItemCommand();

            // Run locally right now
            await context.Invoke(command);

            // Run in the background, let Jasper decide if the message should be durable
            await context.Enqueue(command);

            // or fire-and-forget
            await context.EnqueueLightweight(command);

            // or durably
            await context.EnqueueDurably(command);

            // or run it in the future
            await context.Schedule(command, 5.Minutes());

            // or at a specific time
            await context.Schedule(command, DateTime.Today.AddHours(23));
        }
    }
}
