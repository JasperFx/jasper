using Marten;
using Marten.Schema;
using Marten.Storage;

namespace Jasper.Marten.Persistence.DbObjects
{
    public class OutgoingEnvelopeTable : Table
    {
        public OutgoingEnvelopeTable(StoreOptions options) : base(new DbObjectName(options.DatabaseSchemaName, PostgresqlEnvelopeStorage.OutgoingTableName))
        {
            AddPrimaryKey(new TableColumn("id", "uuid"));
            AddColumn("owner_id", "int", "NOT NULL");
            AddColumn("destination", "varchar");
            AddColumn("deliver_by", "timestamp");
            AddColumn("body", "bytea", "NOT NULL");
        }
    }
}