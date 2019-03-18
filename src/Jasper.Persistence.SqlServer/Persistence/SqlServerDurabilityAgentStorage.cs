using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurabilityAgentStorage : DataAccessor,IDurabilityAgentStorage
    {
        private readonly SqlServerDurableStorageSession _session;
        private readonly string _findReadyToExecuteJobs;
        private readonly CancellationToken _cancellation;

        public SqlServerDurabilityAgentStorage(DatabaseSettings settings, JasperOptions options)
        {
            var transaction = new SqlServerDurableStorageSession(settings, options.Cancellation);

            _session = transaction;
            Session = transaction;

            Nodes = new DurableNodes(transaction, settings, options.Cancellation);
            Incoming = new SqlServerDurableIncoming(transaction, settings, options);
            Outgoing = new SqlServerDurableOutgoing(transaction, settings, options);

            _findReadyToExecuteJobs =
                $"select body from {settings.SchemaName}.{IncomingTable} where status = '{TransportConstants.Scheduled}' and execution_time <= @time";

            _cancellation = options.Cancellation;
        }

        public IDurableStorageSession Session { get; }
        public IDurableNodes Nodes { get; }
        public IDurableIncoming Incoming { get; }
        public IDurableOutgoing Outgoing { get; }

        public Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return _session
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow)
                .ExecuteToEnvelopes(_cancellation, _session.Transaction);
        }

        public void Dispose()
        {
            Session?.Dispose();
        }
    }
}
