using Jasper;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Marten;

namespace StorytellerSpecs.Fixtures.Marten.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [Transaction]
        public void Handle(TraceMessage message, IDocumentSession session)
        {
            session.Store(new TraceDoc {Name = message.Name});
        }
    }
}
