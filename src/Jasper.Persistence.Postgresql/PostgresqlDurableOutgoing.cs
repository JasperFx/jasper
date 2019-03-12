using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Postgresql.Util;
using Jasper.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableOutgoing : PostgresqlAccess,IDurableOutgoing
    {
        private readonly PostgresqlDurableStorageSession _session;
        private readonly string _findUniqueDestinations;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _deleteOutgoingByDestinationSql;
        private readonly string _deleteOutgoingSql;
        private readonly string _reassignSql;

        public PostgresqlDurableOutgoing(PostgresqlDurableStorageSession session, PostgresqlSettings settings, JasperOptions options)
        {
            _session = session;
            _findUniqueDestinations = $"select distinct destination from {settings.SchemaName}.{OutgoingTable}";
            _findOutgoingEnvelopesSql =
                $"select body from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = :destination limit {options.Retries.RecoveryBatchSize}";
            _deleteOutgoingByDestinationSql =
                $"delete from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = :destination";


            _deleteOutgoingSql =
                $"delete from {settings.SchemaName}.{OutgoingTable} where id = ANY(:ids)";

            _reassignSql = $"update {settings.SchemaName}.{OutgoingTable} set owner_id = :owner where id = ANY(:ids)";
        }

        public Task<Envelope[]> Load(Uri destination)
        {
            return _session.CreateCommand(_findOutgoingEnvelopesSql)
                .With("destination", destination.ToString(), NpgsqlDbType.Varchar)
                .ExecuteToEnvelopes();

        }

        public Task Reassign(int ownerId, Envelope[] outgoing)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId, NpgsqlDbType.Integer)
                .With("ids", outgoing.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                .ExecuteNonQueryAsync();
        }

        public Task DeleteByDestination(Uri destination)
        {
            return _session.CreateCommand(_deleteOutgoingByDestinationSql)
                .With("destination", destination.ToString(), NpgsqlDbType.Varchar)
                .ExecuteNonQueryAsync();
        }

        public Task Delete(Envelope[] outgoing)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("ids", outgoing.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                .ExecuteNonQueryAsync();
        }

        public async Task<Uri[]> FindAllDestinations()
        {
            var list = new List<Uri>();

            var cmd = _session.CreateCommand(_findUniqueDestinations);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var text = await reader.GetFieldValueAsync<string>(0);
                    list.Add(text.ToUri());
                }
            }

            return list.ToArray();
        }
    }
}
