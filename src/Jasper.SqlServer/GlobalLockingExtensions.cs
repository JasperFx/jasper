using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Jasper.SqlServer
{
    public static class GlobalLockingExtensions
    {
        public static Task GetGlobalTxLock(this SqlConnection conn, int lockId)
        {
            throw new NotImplementedException();
            //return conn.CreateCommand("SELECT pg_advisory_xact_lock(:id);").With("id", lockId, DbType.Integer).ExecuteNonQueryAsync();
        }

        public static async Task<bool> TryGetGlobalTxLock(this SqlConnection conn, int lockId)
        {
            throw new NotImplementedException();
//            var c = await conn.CreateCommand("SELECT pg_try_advisory_xact_lock(:id);").With("id", lockId, NpgsqlDbType.Integer)
//                .ExecuteScalarAsync();
//
//            return (bool)c;
        }

        public class AdvisoryLock
        {
            public bool Granted { get; }
            public string Grant { get; }
            public DateTime? Start { get; }

            public AdvisoryLock(bool granted, string grant, DateTime? start)
            {
                Granted = granted;
                Grant = grant;
                Start = start;
            }
        }

        public static async Task<IList<AdvisoryLock>> GetOpenLocks(this SqlConnection conn)
        {
            var sql = @"

select pg_locks.granted,
       pg_stat_activity.query,
       pg_stat_activity.query_start
       from pg_locks
  JOIN pg_stat_activity
    on pg_locks.pid = pg_stat_activity.pid
  WHERE pg_locks.locktype = 'advisory';
";

            var list = new List<AdvisoryLock>();
//            using (var reader = await conn.CreateCommand(sql).ExecuteReaderAsync())
//            {
//                while (await reader.ReadAsync())
//                {
//                    var granted = await reader.GetFieldValueAsync<bool>(0);
//                    var grant = await reader.GetFieldValueAsync<string>(1);
//                    var start = await reader.GetFieldValueAsync<DateTime>(2);
//                    var advisoryLock = new AdvisoryLock(granted, grant, start);
//
//                    list.Add(advisoryLock);
//                }
//            }

            return list;
        }

        public static Task GetGlobalLock(this SqlConnection conn, int lockId)
        {
            throw new NotImplementedException();
//            return conn.CreateCommand("SELECT pg_advisory_lock(:id);").With("id", lockId, NpgsqlDbType.Integer)
//                .ExecuteNonQueryAsync();
        }

        public static async Task<bool> TryGetGlobalLock(this SqlConnection conn, int lockId)
        {
            throw new NotImplementedException();
//            var c = await conn.CreateCommand("SELECT pg_try_advisory_lock(:id);").With("id", lockId, DbType.Integer)
//                .ExecuteScalarAsync();
//
//            return (bool)c;
        }

        public static Task ReleaseGlobalLock(this SqlConnection conn, int lockId)
        {
            throw new NotImplementedException();
//            return conn.CreateCommand("SELECT pg_advisory_unlock(:id);").With("id", lockId, DbType.Integer)
//                .ExecuteNonQueryAsync();
        }
    }
}
