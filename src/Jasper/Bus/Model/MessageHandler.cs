using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Model
{
    // SAMPLE: MessageHandler
    public abstract class MessageHandler
    {
        public HandlerChain Chain { get; set; }

        // This method actually processes the incoming Envelope
        public abstract Task Handle(IInvocationContext context);
    }
    // ENDSAMPLE
}
