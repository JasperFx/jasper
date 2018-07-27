using Jasper;
using Jasper.Persistence.Marten;
using Marten;

namespace DurabilitySpecs.Fixtures.Marten.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [MartenTransaction]
        public void Handle(TraceMessage message, IDocumentSession session)
        {
            session.Store(new TraceDoc{Name = message.Name});
        }
    }
}
