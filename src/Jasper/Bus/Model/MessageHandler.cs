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
        public abstract Task Handle(IInvocationContext input);

        public string ToRelativePath(string[] segments, int starting)
        {
            return segments.Skip(starting).Join("/");
        }

        public string[] ToPathSegments(string[] segments, int starting)
        {
            return segments.Skip(starting).ToArray();
        }
    }
    // ENDSAMPLE
}
