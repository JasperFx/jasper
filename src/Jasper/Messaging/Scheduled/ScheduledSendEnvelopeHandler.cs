using System.Threading.Tasks;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Scheduled
{

    public class ScheduledSendEnvelopeHandler : MessageHandler
    {
        public ScheduledSendEnvelopeHandler()
        {
            Chain = new HandlerChain(typeof(Envelope));
        }

        public override Task Handle(IMessageContext context)
        {
            var scheduled = (Envelope)context.Envelope.Message;
            return context.Send(scheduled);
        }
    }
}
