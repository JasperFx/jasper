using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public abstract partial class DatabaseBackedEnvelopePersistence<T>
    {
        public async Task ClearAllPersistedEnvelopes()
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);
            await truncateEnvelopeData(conn);
        }

        [Obsolete("Eliminate in favor of resource model")]
        public async Task RebuildStorageAsync()
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            var migration = await SchemaMigration.Determine(conn, Objects);

            if (migration.Difference != SchemaPatchDifference.None)
            {
                await Migrator.ApplyAll(conn, migration, AutoCreate.CreateOrUpdate);
            }

            await truncateEnvelopeData(conn);
        }

        private async Task truncateEnvelopeData(DbConnection conn)
        {
            try
            {
                var tx = await conn.BeginTransactionAsync(_cancellation);
                await tx.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}").ExecuteNonQueryAsync(_cancellation);
                await tx.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable}").ExecuteNonQueryAsync(_cancellation);
                await tx.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.DeadLetterTable}").ExecuteNonQueryAsync(_cancellation);

                await tx.CommitAsync(_cancellation);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Failure trying to execute the statements to clear envelope storage", e);
            }
        }

        public abstract Task<PersistedCounts> GetPersistedCounts();

        public async Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes()
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            return await conn
                .CreateCommand(
                    $"select {DatabaseConstants.IncomingFields} from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable}")
                .FetchList(r => DatabasePersistence.ReadIncoming(r, _cancellation), cancellation: _cancellation);
        }

        public async Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes()
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            return await conn
                .CreateCommand(
                    $"select {DatabaseConstants.OutgoingFields} from {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable}")
                .FetchList(r => DatabasePersistence.ReadOutgoing(r, _cancellation), cancellation: _cancellation);
        }

        public async Task ReleaseAllOwnership()
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            await conn.CreateCommand($"update {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} set owner_id = 0;update {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = 0").ExecuteNonQueryAsync(_cancellation);

        }
    }
}
