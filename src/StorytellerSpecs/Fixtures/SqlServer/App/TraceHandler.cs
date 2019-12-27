using System.Data.SqlClient;
using Jasper.Attributes;
using Jasper.Persistence.Database;

namespace StorytellerSpecs.Fixtures.SqlServer.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [Transactional]
        public void Handle(TraceMessage message, SqlTransaction tx)
        {
            var traceDoc = new TraceDoc {Name = message.Name};

            tx.CreateCommand("insert into receiver.trace_doc (id, name) values (@id, @name)")
                .With("id", traceDoc.Id)
                .With("name", traceDoc.Name)
                .ExecuteNonQuery();
        }
    }
}
