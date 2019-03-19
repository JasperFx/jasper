using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlSettings : DatabaseSettings
    {

        public PostgresqlSettings() : base("public")
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
            return conn.CreateCommand("SELECT pg_advisory_xact_lock(:id);").With("id", lockId)
                .ExecuteNonQueryAsync(cancellation);
        }

        public override async Task<bool> TryGetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            var c = await conn.CreateCommand("SELECT pg_try_advisory_xact_lock(:id);")
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
            var c = await tx.CreateCommand("SELECT pg_try_advisory_xact_lock(:id);")
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

        public static async Task<IList<AdvisoryLock>> GetOpenLocks(NpgsqlConnection conn)
        {
            var sql = @"

select pg_locks.granted,
       pg_stat_activity.query,
       pg_stat_activity.query_start,
       pg_locks.objid
       from pg_locks
  JOIN pg_stat_activity
    on pg_locks.pid = pg_stat_activity.pid
  WHERE pg_locks.locktype = 'advisory';
";

            var list = new List<AdvisoryLock>();
            using (var reader = await conn.CreateCommand(sql).ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var granted = await reader.GetFieldValueAsync<bool>(0);
                    var grant = await reader.GetFieldValueAsync<string>(1);
                    var start = await reader.GetFieldValueAsync<DateTime>(2);
                    var uniqueId = await reader.GetFieldValueAsync<long>(3);

                    var advisoryLock = new AdvisoryLock(granted, grant, start, uniqueId);


                    list.Add(advisoryLock);
                }
            }

            return list;
        }

        public class AdvisoryLock
        {
            public AdvisoryLock(bool granted, string grant, DateTime? start, long uniqueId)
            {
                Granted = granted;
                Grant = grant;
                Start = start;
                UniqueId = uniqueId;
            }

            public bool Granted { get; }
            public string Grant { get; }
            public DateTime? Start { get; }
            public long UniqueId { get; }
        }

    }


}
