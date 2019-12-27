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

        public override Task Handle(IMessageContext context, CancellationToken cancellation)
        {
            var scheduled = (Envelope)context.Envelope.Message;
            return context.Send(scheduled);
        }
    }
}
