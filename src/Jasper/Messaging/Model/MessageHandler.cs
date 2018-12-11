using System.Threading.Tasks;

namespace Jasper.Messaging.Model
{
    // SAMPLE: MessageHandler
    public abstract class MessageHandler
    {
        public HandlerChain Chain { get; set; }

        // This method actually processes the incoming Envelope
        public abstract Task Handle(IMessageContext context);
    }

    // ENDSAMPLE
}
