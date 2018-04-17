using Marten;
using Marten.Schema;
using Marten.Storage;

namespace Jasper.Marten.Persistence.DbObjects
{
    public class IncomingEnvelopeTable : Table
    {
        public IncomingEnvelopeTable(StoreOptions options) : base(new DbObjectName(options.DatabaseSchemaName, PostgresqlEnvelopeStorage.IncomingTableName))
        {
            AddPrimaryKey(new TableColumn("id", "uuid"));
            AddColumn("status", "varchar", "NOT NULL");
            AddColumn("owner_id", "int", "NOT NULL");
            AddColumn("execution_time", "timestamptz");
            AddColumn("attempts", "int", "DEFAULT 0");
            AddColumn("body", "bytea", "NOT NULL");
        }
    }
}
