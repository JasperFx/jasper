using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime.Handlers;

namespace Jasper.Runtime.Scheduled;

public class ScheduledSendEnvelopeHandler : MessageHandler
{
    public ScheduledSendEnvelopeHandler(HandlerGraph parent)
    {
        Chain = new HandlerChain(typeof(Envelope), parent);
    }

    public override Task HandleAsync(IExecutionContext context, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var scheduled = (Envelope)context.Envelope!.Message!;

        return context.As<ExecutionContext>().ForwardScheduledEnvelopeAsync(scheduled).AsTask();
    }
}
