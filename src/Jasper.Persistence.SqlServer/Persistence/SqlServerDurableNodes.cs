using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableNodes : SqlServerAccess,IDurableNodes
    {
        private readonly SqlServerDurableStorageSession _session;
        private readonly string _fetchOwnersSql;
        private readonly string _reassignDormantNodeSql;

        public SqlServerDurableNodes(SqlServerDurableStorageSession session, SqlServerSettings settings)
        {
            _session = session;
            _fetchOwnersSql = $@"
select distinct owner_id from {settings.SchemaName}.{IncomingTable} where owner_id != 0 and owner_id != @owner
union
select distinct owner_id from {settings.SchemaName}.{OutgoingTable} where owner_id != 0 and owner_id != @owner";

            _reassignDormantNodeSql = $@"
update {settings.SchemaName}.{IncomingTable}
  set owner_id = 0
where
  owner_id = @owner;

update {settings.SchemaName}.{OutgoingTable}
  set owner_id = 0
where
  owner_id = @owner;
";
        }

        public Task ReassignDormantNodeToAnyNode(int nodeId)
        {
            return _session.CreateCommand(_reassignDormantNodeSql)
                .With("owner", nodeId, SqlDbType.Int)
                .ExecuteNonQueryAsync();
        }

        public async Task<int[]> FindUniqueOwners(int currentNodeId)
        {
            var list = new List<int>();
            using (var reader = await _session.CreateCommand(_fetchOwnersSql)
                .With("owner", currentNodeId).ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = await reader.GetFieldValueAsync<int>(0);
                    list.Add(id);
                }
            }

            return list.ToArray();
        }
    }
}
