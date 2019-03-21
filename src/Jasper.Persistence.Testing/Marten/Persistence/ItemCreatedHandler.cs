using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class ItemCreatedHandler
    {
        [Transactional]
        public static void Handle(ItemCreated created, IDocumentSession session, MessageTracker tracker,
            Envelope envelope)
        {
            session.Store(created);
            tracker.Record(created, envelope);
        }
    }
}
