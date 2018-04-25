using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports.Configuration;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();
        private readonly string _reassignDormantNodeSql;

        public ReassignFromDormantNodes(SqlServerSettings marker, MessagingSettings settings)
        {
            _reassignDormantNodeSql = $@"
update {marker.SchemaName}.{SqlServerEnvelopePersistor.IncomingTable}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {marker.SchemaName}.{SqlServerEnvelopePersistor.IncomingTable}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND APPLOCK_TEST ( '{marker.DatabasePrincipal}' , owner_id , 'Exclusive' , 'Transaction' ) = 1
  );

update {marker.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {marker.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND APPLOCK_TEST ( '{marker.DatabasePrincipal}' , owner_id , 'Exclusive' , 'Transaction' ) = 1
  );
";
        }

        public async Task Execute(SqlConnection conn, ISchedulingAgent agent, SqlTransaction tx)
        {
            if (!await conn.TryGetGlobalTxLock(tx, ReassignmentLockId))
            {
                return;
            }

            await conn.CreateCommand(tx, _reassignDormantNodeSql).ExecuteNonQueryAsync();

            tx.Commit();
        }
    }
}
