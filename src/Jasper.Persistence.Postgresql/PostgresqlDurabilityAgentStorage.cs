using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Postgresql.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurabilityAgentStorage : PostgresqlAccess,IDurabilityAgentStorage
    {
        private readonly PostgresqlDurableStorageSession _session;
        private readonly string _findReadyToExecuteJobs;
        private readonly CancellationToken _cancellation;

        public PostgresqlDurabilityAgentStorage(PostgresqlSettings settings, JasperOptions options)
        {
            _session = new PostgresqlDurableStorageSession(settings, options.Cancellation);
            Session = _session;

            Nodes = new PostgresqlDurableNodes(_session, settings, options);
            Incoming = new PostgresqlDurableIncoming(_session, settings, options);
            Outgoing = new PostgresqlDurableOutgoing(_session, settings, options);

            _findReadyToExecuteJobs =
                $"select body from {settings.SchemaName}.{IncomingTable} where status = '{TransportConstants.Scheduled}' and execution_time <= :time";

            _cancellation = options.Cancellation;
        }

        public void Dispose()
        {
            Session.Dispose();
        }

        public IDurableStorageSession Session { get; }
        public IDurableNodes Nodes { get; }
        public IDurableIncoming Incoming { get; }
        public IDurableOutgoing Outgoing { get; }
        public Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return _session.Connection
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow, NpgsqlDbType.TimestampTz)
                .ExecuteToEnvelopes(_cancellation);
        }
    }
}
