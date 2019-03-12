using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Postgresql.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableIncoming : PostgresqlAccess,IDurableIncoming
    {
        private readonly PostgresqlDurableStorageSession _session;
        private PostgresqlSettings _settings;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly string _reassignSql;

        public PostgresqlDurableIncoming(PostgresqlDurableStorageSession session, PostgresqlSettings settings, JasperOptions options)
        {
            _session = session;
            _settings = settings;
            _findAtLargeEnvelopesSql =
                $"select body from {settings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}' limit {options.Retries.RecoveryBatchSize}";

            _reassignSql =
                $"UPDATE {_settings.SchemaName}.{IncomingTable} SET owner_id = :owner, status = '{TransportConstants.Incoming}' WHERE id = ANY(:ids)";
        }

        public Task<Envelope[]> LoadPageOfLocallyOwned()
        {
            return _session
                .CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes();
        }

        public Task Reassign(int ownerId, Envelope[] incoming)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId, NpgsqlDbType.Integer)
                .With("ids", incoming.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                .ExecuteNonQueryAsync();

        }
    }
}
