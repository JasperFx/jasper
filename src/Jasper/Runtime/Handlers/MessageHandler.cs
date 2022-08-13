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

    public abstract Task HandleAsync(IExecutionContext context, CancellationToken cancellation);
}

#endregion
