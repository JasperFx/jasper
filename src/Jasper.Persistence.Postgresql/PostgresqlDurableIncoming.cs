using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
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

        public PostgresqlDurableIncoming(DurableStorageSession session, DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            _session = session;
            _findAtLargeEnvelopesSql =
                $"select body from {databaseSettings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{EnvelopeStatus.Incoming}' limit {settings.RecoveryBatchSize}";

            _reassignSql =
                $"UPDATE {databaseSettings.SchemaName}.{IncomingTable} SET owner_id = @owner, status = '{EnvelopeStatus.Incoming}' WHERE id = ANY(@ids)";

            _cancellation = settings.Cancellation;
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
