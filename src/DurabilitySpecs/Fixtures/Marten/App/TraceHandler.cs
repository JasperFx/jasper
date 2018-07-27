using Jasper;
using Jasper.Marten;
using Jasper.Persistence.Marten;
using Marten;
using Newtonsoft.Json;

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
