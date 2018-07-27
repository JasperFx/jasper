﻿using System.Data.SqlClient;
using Jasper;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Util;
using Jasper.SqlServer;


namespace DurabilitySpecs.Fixtures.SqlServer.App
{
    [JasperIgnore]
    public class TraceHandler
    {
        [SqlTransaction]
        public void Handle(TraceMessage message, SqlTransaction tx)
        {
            var traceDoc = new TraceDoc{Name = message.Name};

            tx.Connection.CreateCommand(tx, "insert into receiver.trace_doc (id, name) values (@id, @name)")
                .With("id", traceDoc.Id)
                .With("name", traceDoc.Name)
                .ExecuteNonQuery();
        }
    }
}
