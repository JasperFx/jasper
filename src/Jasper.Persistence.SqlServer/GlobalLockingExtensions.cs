using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer
{
    public static class GlobalLockingExtensions
    {
        public static Task GetGlobalTxLock(this DbConnection conn, SqlTransaction tx, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return getLock(conn, lockId, "Transaction", tx, cancellation);
        }

        private static async Task getLock(DbConnection conn, int lockId, string owner, SqlTransaction tx,
            CancellationToken cancellation)
        {
            var returnValue = await tryGetLock(conn, lockId, owner, tx, cancellation);

            if (returnValue < 0)
                throw new Exception($"sp_getapplock failed with errorCode '{returnValue}'");
        }

        private static async Task<int> tryGetLock(DbConnection conn, int lockId, string owner, SqlTransaction tx,
            CancellationToken cancellation)
        {
            var cmd = conn.CreateCommand("sp_getapplock");
            cmd.Transaction = tx;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.With("Resource", lockId.ToString());
            cmd.With("LockMode", "Exclusive");

            cmd.With("LockOwner", owner);
            cmd.With("LockTimeout", 1000);

            var returnValue = cmd.CreateParameter();
            returnValue.ParameterName = "ReturnValue";
            returnValue.DbType = DbType.Int32;
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(returnValue);

            await cmd.ExecuteNonQueryAsync(cancellation);

            return (int) returnValue.Value;
        }

        public static async Task<bool> TryGetGlobalTxLock(this DbConnection conn, SqlTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Transaction", tx, cancellation) >= 0;
        }


        public static Task GetGlobalLock(this DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            SqlTransaction transaction = null)
        {
            return getLock(conn, lockId, "Session", transaction, cancellation);
        }

        public static async Task<bool> TryGetGlobalLock(this DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", null, cancellation) >= 0;
        }

        public static async Task<bool> TryGetGlobalLock(this DbConnection conn, int lockId, SqlTransaction tx,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", tx, cancellation) >= 0;
        }

        public static Task ReleaseGlobalLock(this DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null)
        {
            var sqlCommand = conn.CreateCommand("sp_releaseapplock");
            sqlCommand.Transaction = tx;
            sqlCommand.CommandType = CommandType.StoredProcedure;

            sqlCommand.With("Resource", lockId.ToString());
            sqlCommand.With("LockOwner", "Session");

            return sqlCommand.ExecuteNonQueryAsync(cancellation);
        }

        public class AdvisoryLock
        {
            public AdvisoryLock(bool granted, string grant, DateTime? start)
            {
                Granted = granted;
                Grant = grant;
                Start = start;
            }

            public bool Granted { get; }
            public string Grant { get; }
            public DateTime? Start { get; }
        }


    }
}
