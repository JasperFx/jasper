using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class NoHandlerContinuation : IContinuation
{
    private readonly IMissingHandler[] _handlers;
    private readonly IJasperRuntime _root;

    public NoHandlerContinuation(IMissingHandler[] handlers, IJasperRuntime root)
    {
        _handlers = handlers;
        _root = root;
    }

    public async ValueTask ExecuteAsync(IExecutionContext execution,
        IJasperRuntime runtime,
        DateTimeOffset now)
    {
        if (execution.Envelope == null) throw new InvalidOperationException("Context does not have an Envelope");

        execution.Logger.NoHandlerFor(execution.Envelope!);

        foreach (var handler in _handlers)
        {
            try
            {
                await handler.HandleAsync(execution, _root);
            }
            catch (Exception? e)
            {
                execution.Logger.LogException(e);
            }
        }

        if (execution.Envelope.AckRequested)
        {
            await execution.SendAcknowledgementAsync();
        }

        await execution.CompleteAsync();

        // These two lines are important to make the message tracking work
        // if there is no handler
        execution.Logger.ExecutionFinished(execution.Envelope);
        execution.Logger.MessageSucceeded(execution.Envelope);
    }
}
