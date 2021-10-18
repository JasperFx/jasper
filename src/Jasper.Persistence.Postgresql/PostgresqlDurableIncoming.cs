using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql.Util;
using Jasper.Transports;
using NpgsqlTypes;
using Weasel.Core;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableIncoming : IDurableIncoming
    {
        private readonly DurableStorageSession _session;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly string _reassignSql;
        private readonly CancellationToken _cancellation;

        public PostgresqlDurableIncoming(DurableStorageSession session, DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            _session = session;
            _findAtLargeEnvelopesSql =
                $"select body, attempts from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{EnvelopeStatus.Incoming}' limit {settings.RecoveryBatchSize}";

            _reassignSql =
                $"UPDATE {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} SET owner_id = @owner, status = '{EnvelopeStatus.Incoming}' WHERE id = ANY(@ids)";

            _cancellation = settings.Cancellation;
        }

        public Task<Envelope[]> LoadPageOfLocallyOwnedIncoming()
        {
            return _session
                .CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopesWithAttempts(_cancellation);
        }

        public Task ReassignIncoming(int ownerId, Envelope[] incoming)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId)
                .With("ids", incoming)
                .ExecuteNonQueryAsync(_cancellation);

        }
    }
}
