using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Npgsql;
using NpgsqlTypes;
using Weasel.Core;
using Weasel.Postgresql;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlSettings : DatabaseSettings
    {

        public PostgresqlSettings() : base("public", new PostgresqlMigrator())
        {
        }

        public override DbConnection CreateConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        public override DbCommand CreateEmptyCommand()
        {
            return new NpgsqlCommand();
        }

        public override Task GetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            return tx.CreateCommand("SELECT pg_advisory_xact_lock(:id);").With("id", lockId)
                .ExecuteNonQueryAsync(cancellation);
        }

        public override async Task<bool> TryGetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            var c = await tx.CreateCommand("SELECT pg_try_advisory_xact_lock(:id);")
                .With("id", lockId)
                .ExecuteScalarAsync(cancellation);

            return (bool) c;
        }

        public override Task GetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction transaction = null)
        {
            return conn.CreateCommand("SELECT pg_advisory_lock(:id);").With("id", lockId)
                .ExecuteNonQueryAsync(cancellation);
        }

        public override async Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            var c = await conn.CreateCommand("SELECT pg_try_advisory_lock(:id);")
                .With("id", lockId)
                .ExecuteScalarAsync(cancellation);

            return (bool) c;
        }

        public override async Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, DbTransaction tx,
            CancellationToken cancellation = default(CancellationToken))
        {
            var c = await conn.CreateCommand("SELECT pg_try_advisory_xact_lock(:id);")
                .With("id", lockId)
                .ExecuteScalarAsync(cancellation);

            return (bool) c;
        }

        public override Task ReleaseGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null)
        {
            return conn.CreateCommand("SELECT pg_advisory_unlock(:id);").With("id", lockId)
                .ExecuteNonQueryAsync(cancellation);
        }




    }


}
