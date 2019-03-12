using System;
using System.Data;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurabilityAgentStorage : SqlServerAccess,IDurabilityAgentStorage
    {
        private SqlServerDurableStorageSession _session;
        private readonly string _findReadyToExecuteJobs;

        public SqlServerDurabilityAgentStorage(SqlServerSettings settings, JasperOptions options)
        {
            var transaction = new SqlServerDurableStorageSession(settings);

            _session = transaction;
            Session = transaction;

            Nodes = new SqlServerDurableNodes(transaction, settings);
            Incoming = new SqlServerDurableIncoming(transaction, settings, options);
            Outgoing = new SqlServerDurableOutgoing(transaction, settings, options);

            _findReadyToExecuteJobs =
                $"select body from {settings.SchemaName}.{IncomingTable} where status = '{TransportConstants.Scheduled}' and execution_time <= @time";
        }

        public IDurableStorageSession Session { get; }
        public IDurableNodes Nodes { get; }
        public IDurableIncoming Incoming { get; }
        public IDurableOutgoing Outgoing { get; }

        public Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return _session
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow, SqlDbType.DateTimeOffset)
                .ExecuteToEnvelopes(_session.Transaction);
        }

        public void Dispose()
        {
            Session?.Dispose();
        }
    }
}
