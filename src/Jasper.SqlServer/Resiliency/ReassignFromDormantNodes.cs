using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        private readonly MessagingSettings _settings;
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();
        private readonly string _reassignDormantNodeSql;
        private readonly string _fetchOwnersSql;

        public ReassignFromDormantNodes(SqlServerSettings marker, MessagingSettings settings)
        {
            _settings = settings;
            _fetchOwnersSql = $@"
select distinct owner_id from {marker.SchemaName}.{SqlServerEnvelopePersistor.IncomingTable} where owner_id != 0 and owner_id != @owner
union
select distinct owner_id from {marker.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable} where owner_id != 0 and owner_id != @owner";

            _reassignDormantNodeSql = $@"
update {marker.SchemaName}.{SqlServerEnvelopePersistor.IncomingTable}
  set owner_id = 0
where
  owner_id = @owner;

update {marker.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable}
  set owner_id = 0
where
  owner_id = @owner;
";
        }

        public async Task Execute(SqlConnection conn, ISchedulingAgent agent)
        {
            var tx = conn.BeginTransaction();

            if (!await conn.TryGetGlobalTxLock(tx, ReassignmentLockId))
            {
                tx.Rollback();
                return;
            }

            try
            {
                var owners = await fetchOwners(tx);

                foreach (var owner in owners.Distinct())
                {
                    if (owner == _settings.UniqueNodeId) continue;

                    if (await conn.TryGetGlobalTxLock(tx, owner))
                    {
                        await conn.CreateCommand(tx, _reassignDormantNodeSql)
                            .With("owner", owner, SqlDbType.Int)
                            .ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
                tx.Rollback();
                throw;
            }

            tx.Commit();
        }

        private async Task<List<int>> fetchOwners(SqlTransaction tx)
        {
            var list = new List<int>();
            using (var reader = (await tx.Connection.CreateCommand(tx, _fetchOwnersSql).With("owner", _settings.UniqueNodeId).ExecuteReaderAsync()))
            {
                while (await reader.ReadAsync())
                {
                    var id = await reader.GetFieldValueAsync<int>(0);
                    list.Add(id);
                }
            }

            return list;
        }
    }
}
