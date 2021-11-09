using Jasper.Attributes;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Durability
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