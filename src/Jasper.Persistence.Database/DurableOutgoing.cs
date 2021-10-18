using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public abstract class DurableOutgoing : IDurableOutgoing
    {
        public DatabaseSettings DatabaseSettings { get; }
        public AdvancedSettings Settings { get; }
        private readonly IDatabaseSession _session;
        private readonly string _findUniqueDestinations;
        private readonly string _deleteOutgoingSql;
        private readonly CancellationToken _cancellation;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _insertOutgoingSql;


        protected DurableOutgoing(IDatabaseSession session, DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            DatabaseSettings = databaseSettings;
            Settings = settings;
            _session = session;
            _findUniqueDestinations =
                $"select distinct destination from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}";

            _deleteOutgoingSql =
                $"delete from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = :owner and destination = @destination";

            _cancellation = settings.Cancellation;

            _findOutgoingEnvelopesSql =
                determineOutgoingEnvelopeSql(databaseSettings, settings);

            _insertOutgoingSql =
                $"insert into {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@id, @owner, @destination, @deliverBy, @body)";

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

        public Task DeleteOutgoing(Envelope envelope)
        {
            return DatabaseSettings
                .CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
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

        public abstract Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId);
        public abstract Task DeleteOutgoing(Envelope[] envelopes);

        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            return DatabaseSettings.CreateCommand(_insertOutgoingSql)
                .With("id", envelope.Id)
                .With("owner", ownerId)
                .With("destination", envelope.Destination.ToString())
                .With("deliverBy", envelope.DeliverBy)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, DatabaseSettings);
            return cmd.ExecuteOnce(CancellationToken.None);
        }

        public Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, Settings.UniqueNodeId, DatabaseSettings);
            cmd.Connection = tx.Connection;
            cmd.Transaction = tx;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

            public static DbCommand BuildOutgoingStorageCommand(Envelope[] envelopes, int ownerId,
                DatabaseSettings settings)
            {
                var builder = settings.ToCommandBuilder();

                builder.AddNamedParameter("owner", ownerId).DbType = DbType.Int32;

                foreach (var envelope in envelopes)
                {
                    var id = builder.AddParameter(envelope.Id);
                    var destination = builder.AddParameter(envelope.Destination.ToString());
                    var deliverBy = builder.AddParameter(envelope.DeliverBy);
                    var body = builder.AddParameter(envelope.Serialize());

                    builder.Append(
                        $"insert into {settings.SchemaName}.{DatabaseConstants.OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@{id.ParameterName}, @owner, @{destination.ParameterName}, @{deliverBy.ParameterName}, @{body.ParameterName});");
                }

                return builder.Compile();
            }

    }
}
