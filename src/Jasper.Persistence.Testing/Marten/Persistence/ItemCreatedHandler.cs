using Jasper.Attributes;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class ItemCreatedHandler
    {
        [Transactional]
        public static void Handle(ItemCreated created, IDocumentSession session,
            Envelope envelope)
        {
            session.Store(created);
        }
    }
}
