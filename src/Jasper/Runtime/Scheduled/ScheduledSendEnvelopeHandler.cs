using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Runtime.Handlers;

namespace Jasper.Runtime.Scheduled
{

    public class ScheduledSendEnvelopeHandler : MessageHandler
    {

        public ScheduledSendEnvelopeHandler()
        {
            Chain = new HandlerChain(typeof(Envelope));
        }

        public override Task Handle(IExecutionContext context, CancellationToken cancellation)
        {
            if (cancellation.IsCancellationRequested) return Task.CompletedTask;
            var scheduled = (Envelope)context.Envelope.Message;

            return context.SendEnvelopeAsync(scheduled);
        }
    }
}
