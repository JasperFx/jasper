using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public abstract class DurableOutgoing : DataAccessor, IDurableOutgoing
    {
        private readonly IDatabaseSession _session;
        private readonly string _findUniqueDestinations;
        private readonly string _deleteOutgoingSql;
        private readonly CancellationToken _cancellation;
        private readonly string _findOutgoingEnvelopesSql;


        protected DurableOutgoing(IDatabaseSession session, DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            _session = session;
            _findUniqueDestinations =
                $"select distinct destination from {databaseSettings.SchemaName}.{OutgoingTable}";

            _deleteOutgoingSql =
                $"delete from {databaseSettings.SchemaName}.{OutgoingTable} where owner_id = :owner and destination = @destination";

            _cancellation = settings.Cancellation;

            _findOutgoingEnvelopesSql =
                determineOutgoingEnvelopeSql(databaseSettings, settings);

        }

        protected abstract string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings, AdvancedSettings settings);

        public Task<Envelope[]> Load(Uri destination)
        {
            return _session.CreateCommand(_findOutgoingEnvelopesSql)
                .With("destination", destination.ToString())
                .ExecuteToEnvelopes(_cancellation);
        }

        public abstract Task Reassign(int ownerId, Envelope[] outgoing);

        public Task DeleteByDestination(Uri destination)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("destination", destination.ToString())
                .With("owner", TransportConstants.AnyNode)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public abstract Task Delete(Envelope[] outgoing);

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
