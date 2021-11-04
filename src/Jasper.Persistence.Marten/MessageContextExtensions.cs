using System.Threading.Tasks;
using Marten;

namespace Jasper.Persistence.Marten
{
    public static class MessageContextExtensions
    {
        /// <summary>
        ///     Enlists the current IExecutionContext in the Marten session's transaction
        ///     lifecycle
        /// </summary>
        /// <param name="context"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Task EnlistInTransaction(this IExecutionContext context, IDocumentSession session)
        {
            var persistor = new MartenEnvelopeTransaction(session, context);
            session.Listeners.Add(new FlushOutgoingMessagesOnCommit(context));

            return context.EnlistInTransaction(persistor);
        }
    }
}
