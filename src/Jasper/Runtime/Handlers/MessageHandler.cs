using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Runtime.Handlers;

#region sample_MessageHandler

public interface IMessageHandler
{
    Task HandleAsync(IExecutionContext context, CancellationToken cancellation);
}

public abstract class MessageHandler : IMessageHandler
{
    public HandlerChain? Chain { get; set; }

    // This method actually processes the incoming Envelope
    public abstract Task HandleAsync(IExecutionContext context, CancellationToken cancellation);
}

#endregion
