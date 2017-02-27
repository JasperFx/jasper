using System.Threading.Tasks;
using Jasper.Internal;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Model
{
    public abstract class MessageHandler : IHandler<IInvocationContext>
    {
        public HandlerChain Chain { get; set; }

        public abstract Task Handle(IInvocationContext input);
    }
}