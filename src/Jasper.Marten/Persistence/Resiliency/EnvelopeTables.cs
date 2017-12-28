using System.Threading.Tasks;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Marten;
using Marten.Schema;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class EnvelopeTables
    {
        private readonly string _markOwnedIncomingSql;

        public EnvelopeTables(BusSettings settings, StoreOptions storeConfiguration)
        {
            Incoming = new DbObjectName(storeConfiguration.DatabaseSchemaName,
                PostgresqlEnvelopeStorage.IncomingTableName);
            Outgoing = new DbObjectName(storeConfiguration.DatabaseSchemaName,
                PostgresqlEnvelopeStorage.OutgoingTableName);



            _markOwnedIncomingSql = $"update {Incoming} set owner_id = :owner where id = ANY(:idlist)";

            CurrentNodeId = settings.UniqueNodeId;

        }

        public int CurrentNodeId { get; }

        public DbObjectName Incoming { get; }

        public DbObjectName Outgoing { get; }
    }
}
