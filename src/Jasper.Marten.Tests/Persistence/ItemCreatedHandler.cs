using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging;
using Marten;

namespace Jasper.Marten.Tests.Persistence
{
    public class ItemCreatedHandler
    {
        [MartenTransaction]
        public static void Handle(ItemCreated created, IDocumentSession session, MessageTracker tracker, Envelope envelope)
        {
            session.Store(created);
            tracker.Record(created, envelope);
        }
    }
}
