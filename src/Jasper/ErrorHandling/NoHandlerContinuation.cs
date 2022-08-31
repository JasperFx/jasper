using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class NoHandlerContinuation : IContinuation
{
    private readonly IReadOnlyList<IMissingHandler> _handlers;
    private readonly IJasperRuntime _root;

    public NoHandlerContinuation(IReadOnlyList<IMissingHandler> handlers, IJasperRuntime root)
    {
        _handlers = handlers;
        _root = root;
    }

    public async ValueTask ExecuteAsync(IMessageContext context,
        IJasperRuntime runtime,
        DateTimeOffset now)
    {
        if (context.Envelope == null) throw new InvalidOperationException("Context does not have an Envelope");

        context.Logger.NoHandlerFor(context.Envelope!);

        foreach (var handler in _handlers)
        {
            try
            {
                await handler.HandleAsync(context, _root);
            }
            catch (Exception? e)
            {
                context.Logger.LogException(e);
            }
        }

        if (context.Envelope.AckRequested)
        {
            await context.SendAcknowledgementAsync();
        }

        await context.CompleteAsync();

        // These two lines are important to make the message tracking work
        // if there is no handler
        context.Logger.ExecutionFinished(context.Envelope);
        context.Logger.MessageSucceeded(context.Envelope);
    }
}
