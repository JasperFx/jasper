using Jasper.Attributes;
using Marten;

namespace Jasper.Persistence.Testing.Marten.Durability.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [Transactional]
        public void Handle(TraceMessage message, IDocumentSession session)
        {
            session.Store(new TraceDoc {Name = message.Name});
        }
    }
}
