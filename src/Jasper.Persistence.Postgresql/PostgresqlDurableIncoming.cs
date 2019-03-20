using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableIncoming : DataAccessor,IDurableIncoming
    {
        private readonly DurableStorageSession _session;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly string _reassignSql;
        private readonly CancellationToken _cancellation;

        public PostgresqlDurableIncoming(DurableStorageSession session, DatabaseSettings settings, JasperOptions options)
        {
            _session = session;
            _findAtLargeEnvelopesSql =
                $"select body from {settings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}' limit {options.Retries.RecoveryBatchSize}";

            _reassignSql =
                $"UPDATE {settings.SchemaName}.{IncomingTable} SET owner_id = @owner, status = '{TransportConstants.Incoming}' WHERE id = ANY(@ids)";

            _cancellation = options.Cancellation;
        }

        public Task<Envelope[]> LoadPageOfLocallyOwned()
        {
            return _session
                .CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes(_cancellation);
        }

        public Task Reassign(int ownerId, Envelope[] incoming)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId)
                .With("ids", incoming)
                .ExecuteNonQueryAsync(_cancellation);

        }
    }
}
