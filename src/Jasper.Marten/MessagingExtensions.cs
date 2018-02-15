using System.Threading.Tasks;
using Jasper.Messaging;
using Marten;

namespace Jasper.Marten
{
    public static class MessagingExtensions
    {
        public static Task EnlistInTransaction(this IMessageContext bus, IDocumentSession session)
        {
            var persistor = new MartenEnvelopePersistor(session, bus);
            session.Listeners.Add(new FlushOutgoingMessagesOnCommit(bus));

            return bus.EnlistInTransaction(persistor);
        }
    }
}