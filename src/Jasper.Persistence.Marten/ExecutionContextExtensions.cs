using System;
using System.Threading.Tasks;
using Marten;

namespace Jasper.Persistence.Marten;

public static class ExecutionContextExtensions
{
    /// <summary>
    ///     Enlists the current IExecutionContext in the Marten session's transaction
    ///     lifecycle
    /// </summary>
    /// <param name="context"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public static Task EnlistInTransactionAsync(this IExecutionContext context, IDocumentSession session)
    {
        if (context.Transaction is MartenEnvelopeTransaction)
        {
            throw new InvalidOperationException(
                "This execution context is already enrolled in a Marten Envelope Transaction");
        }

        var persistence = new MartenEnvelopeTransaction(session, context);
        session.Listeners.Add(new FlushOutgoingMessagesOnCommit(context));

        return context.EnlistInTransactionAsync(persistence);
    }
}
