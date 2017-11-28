using Jasper;
using Jasper.Marten;
using Marten;

namespace DurabilitySpecs.Fixtures.Marten.App
{
    public class TraceHandler
    {
        [MartenTransaction, JasperIgnore]
        public void Handle(TraceMessage message, IDocumentSession session)
        {
            session.Store(new TraceDoc{Name = message.Name});
        }
    }
}