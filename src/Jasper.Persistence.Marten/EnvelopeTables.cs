using Jasper.Persistence.Marten.Persistence.DbObjects;
using Marten;
using Marten.Schema;

namespace Jasper.Persistence.Marten
{
    public class EnvelopeTables
    {
        public EnvelopeTables(JasperOptions settings, StoreOptions storeConfiguration)
        {
            Incoming = new DbObjectName(storeConfiguration.DatabaseSchemaName,
                PostgresqlEnvelopeStorage.IncomingTableName);
            Outgoing = new DbObjectName(storeConfiguration.DatabaseSchemaName,
                PostgresqlEnvelopeStorage.OutgoingTableName);

            CurrentNodeId = settings.UniqueNodeId;

            ServiceName = settings.ServiceName;

            SchemaName = storeConfiguration.DatabaseSchemaName;
        }

        public string SchemaName {get;}

        public string ServiceName { get; set; }

        public int CurrentNodeId { get; }

        public DbObjectName Incoming { get; }

        public DbObjectName Outgoing { get; }
    }
}
