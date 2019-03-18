using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableOutgoing : DataAccessor, IDurableOutgoing
    {
        private readonly SqlServerDurableStorageSession _session;
        private readonly SqlServerSettings _settings;
        private readonly string _findUniqueDestinations;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _deleteOutgoingSql;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableOutgoing(SqlServerDurableStorageSession session, SqlServerSettings settings, JasperOptions options)
        {
            _session = session;
            _settings = settings;
            _findUniqueDestinations =
                $"select distinct destination from {settings.SchemaName}.{OutgoingTable}";
            _findOutgoingEnvelopesSql =
                $"select top {options.Retries.RecoveryBatchSize} body from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
            _deleteOutgoingSql =
                $"delete from {settings.SchemaName}.{OutgoingTable} where owner_id = :owner and destination = @destination";

            _cancellation = options.Cancellation;
        }

        public Task<Envelope[]> Load(Uri destination)
        {
            return _session.CreateCommand(_findOutgoingEnvelopesSql)
                .With("destination", destination.ToString())
                .ExecuteToEnvelopes(_cancellation);
        }

        public Task Reassign(int ownerId, Envelope[] outgoing)
        {
            var cmd = _session.CallFunction("uspMarkOutgoingOwnership")
                .WithIdList(_settings, outgoing)
                .With("owner", ownerId);

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public Task DeleteByDestination(Uri destination)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("destination", destination.ToString())
                .With("owner", TransportConstants.AnyNode)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public Task Delete(Envelope[] outgoing)
        {
            return _session
                .CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_settings, outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public async Task<Uri[]> FindAllDestinations()
        {
            var list = new List<Uri>();

            var cmd = _session.CreateCommand(_findUniqueDestinations);
            using (var reader = await cmd.ExecuteReaderAsync(_cancellation))
            {
                while (await reader.ReadAsync(_cancellation))
                {
                    var text = await reader.GetFieldValueAsync<string>(0, _cancellation);
                    list.Add(text.ToUri());
                }
            }

            return list.ToArray();
        }
    }
}
