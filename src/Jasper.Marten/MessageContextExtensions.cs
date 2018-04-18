using System.Threading.Tasks;
using Jasper.Messaging;
using Marten;

namespace Jasper.Marten
{
    public static class MessageContextExtensions
    {
        /// <summary>
        /// Enlists the current IMessageContext in the Marten session's transaction
        /// lifecycle
        /// </summary>
        /// <param name="context"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Task EnlistInTransaction(this IMessageContext context, IDocumentSession session)
        {
            var persistor = new MartenEnvelopeTransaction(session, context);
            session.Listeners.Add(new FlushOutgoingMessagesOnCommit(context));

            return context.EnlistInTransaction(persistor);
        }
    }
}
