using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public class DurableNodes : IDurableNodes
    {
        private readonly IDatabaseSession _session;
        private readonly CancellationToken _cancellation;
        private readonly string _fetchOwnersSql;
        private readonly string _reassignDormantNodeSql;

        public DurableNodes(IDatabaseSession session, DatabaseSettings settings,
            CancellationToken cancellation)
        {
            _session = session;
            _cancellation = cancellation;
            _fetchOwnersSql = $@"
select distinct owner_id from {settings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id != 0 and owner_id != @owner
union
select distinct owner_id from {settings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id != 0 and owner_id != @owner";

            _reassignDormantNodeSql = $@"
update {settings.SchemaName}.{DatabaseConstants.IncomingTable}
  set owner_id = 0
where
  owner_id = @owner;

update {settings.SchemaName}.{DatabaseConstants.OutgoingTable}
  set owner_id = 0
where
  owner_id = @owner;
";
        }

        public Task ReassignDormantNodeToAnyNode(int nodeId)
        {
            return _session.CreateCommand(_reassignDormantNodeSql)
                .With("owner", nodeId)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public async Task<int[]> FindUniqueOwners(int currentNodeId)
        {
            var list = new List<int>();
            using (var reader = await _session.CreateCommand(_fetchOwnersSql)
                .With("owner", currentNodeId)
                .ExecuteReaderAsync(_cancellation))
            {
                while (await reader.ReadAsync(_cancellation))
                {
                    var id = await reader.GetFieldValueAsync<int>(0, _cancellation);
                    list.Add(id);
                }
            }

            return list.ToArray();
        }
    }
}
