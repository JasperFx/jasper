using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public abstract class DurabilityAgentStorage : IDurabilityAgentStorage
    {
        private readonly DurableStorageSession _session;
        private readonly string _findReadyToExecuteJobs;
        private readonly CancellationToken _cancellation;
        private readonly string _fetchOwnersSql;
        private readonly string _reassignDormantNodeSql;


        protected DurabilityAgentStorage(DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            var transaction = new DurableStorageSession(databaseSettings, settings.Cancellation);

            _session = transaction;
            Session = transaction;

            // ReSharper disable once VirtualMemberCallInConstructor
            Incoming = buildDurableIncoming(transaction, databaseSettings, settings);

            // ReSharper disable once VirtualMemberCallInConstructor
            Outgoing = buildDurableOutgoing(transaction, databaseSettings, settings);

            _findReadyToExecuteJobs =
                $"select body, attempts from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where status = '{EnvelopeStatus.Scheduled}' and execution_time <= @time";

            _cancellation = settings.Cancellation;

            _fetchOwnersSql = $@"
select distinct owner_id from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id != 0 and owner_id != @owner
union
select distinct owner_id from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id != 0 and owner_id != @owner";

            _reassignDormantNodeSql = $@"
update {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable}
  set owner_id = 0
where
  owner_id = @owner;

update {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}
  set owner_id = 0
where
  owner_id = @owner;
";
        }

        protected abstract IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings, AdvancedSettings settings);

        protected abstract IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings, AdvancedSettings settings);

        public IDurableStorageSession Session { get; }
        public IDurableIncoming Incoming { get; }
        public IDurableOutgoing Outgoing { get; }

        public Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return _session
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow)
                .ExecuteToEnvelopesWithAttempts(_cancellation, _session.Transaction);
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

        public void Dispose()
        {
            Session?.Dispose();
        }
    }
}
