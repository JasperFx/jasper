using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Runtime.Handlers
{
    #region sample_MessageHandler
    public abstract class MessageHandler
    {
        public HandlerChain Chain { get; set; }

        // This method actually processes the incoming Envelope
        public abstract Task Handle(IExecutionContext context, CancellationToken cancellation);
    }

    #endregion
}
