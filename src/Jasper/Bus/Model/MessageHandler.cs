using System.Threading.Tasks;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Model
{
    public abstract class MessageHandler
    {
        public HandlerChain Chain { get; set; }

        public abstract Task Handle(IInvocationContext input);
    }
}