using System.Threading.Tasks;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Model
{
    public abstract class MessageHandler
    {
        public HandlerChain Chain { get; set; }

        public abstract Task Handle(IInvocationContext input);
    }
}