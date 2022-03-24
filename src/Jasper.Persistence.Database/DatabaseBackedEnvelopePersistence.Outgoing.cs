using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Transports;
using Jasper.Util;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public abstract partial class DatabaseBackedEnvelopePersistence<T>
    {
        public abstract Task DiscardAndReassignOutgoingAsync(Envelope?[] discards, Envelope?[] reassigned, int nodeId);
        public abstract Task DeleteOutgoingAsync(Envelope?[] envelopes);

        protected abstract string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings, AdvancedSettings settings);

        public Task<IReadOnlyList<Envelope?>> LoadOutgoingAsync(Uri? destination)
        {
            return Session.Transaction.CreateCommand(_outgoingEnvelopeSql)
                .With("destination", destination.ToString())
                .FetchList(r => DatabasePersistence.ReadOutgoing(r, _cancellation), cancellation: _cancellation);
        }

        public abstract Task ReassignOutgoingAsync(int ownerId, Envelope?[] outgoing);

        public Task DeleteByDestinationAsync(Uri? destination)
        {
            return Session.Transaction.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = :owner and destination = @destination")
                .With("destination", destination.ToString())
                .With("owner", TransportConstants.AnyNode)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public Task DeleteOutgoingAsync(Envelope? envelope)
        {
            return DatabaseSettings
                .CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public async Task<Uri?[]> FindAllDestinationsAsync()
        {
            var cmd = Session.CreateCommand($"select distinct destination from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}");
            var uris = await cmd.FetchList<string>(cancellation: _cancellation);
            return uris.Select(x => x.ToUri()).ToArray();
        }

        public Task StoreOutgoingAsync(Envelope? envelope, int ownerId)
        {
            return DatabasePersistence.BuildOutgoingStorageCommand(envelope, ownerId, DatabaseSettings)
                .ExecuteOnce(_cancellation);
        }


        public Task StoreOutgoingAsync(Envelope[] envelopes, int ownerId)
        {
            var cmd = DatabasePersistence.BuildOutgoingStorageCommand(envelopes, ownerId, DatabaseSettings);
            return cmd.ExecuteOnce(CancellationToken.None);
        }

        public Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = DatabasePersistence.BuildOutgoingStorageCommand(envelopes, Settings.UniqueNodeId, DatabaseSettings);
            cmd.Connection = tx.Connection;
            cmd.Transaction = tx;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }


    }
}
