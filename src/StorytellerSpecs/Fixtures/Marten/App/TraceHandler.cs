using Jasper;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence;
using Marten;

namespace StorytellerSpecs.Fixtures.Marten.App
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
