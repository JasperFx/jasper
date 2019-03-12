using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer
{
    public static class GlobalLockingExtensions
    {
        public static Task GetGlobalTxLock(this SqlConnection conn, SqlTransaction tx, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return getLock(conn, lockId, "Transaction", tx, cancellation);
        }

        private static async Task getLock(SqlConnection conn, int lockId, string owner, SqlTransaction tx,
            CancellationToken cancellation)
        {
            var returnValue = await tryGetLock(conn, lockId, owner, tx, cancellation);

            if (returnValue < 0)
                throw new Exception(string.Format("sp_getapplock failed with errorCode '{0}'",
                    returnValue));
        }

        private static async Task<int> tryGetLock(SqlConnection conn, int lockId, string owner, SqlTransaction tx,
            CancellationToken cancellation)
        {
            var cmd = conn.CreateCommand("sp_getapplock");
            cmd.Transaction = tx;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("Resource", lockId.ToString()).SqlDbType = SqlDbType.VarChar;
            cmd.Parameters.AddWithValue("LockMode", "Exclusive").SqlDbType = SqlDbType.VarChar;

            cmd.Parameters.AddWithValue("LockOwner", owner).SqlDbType = SqlDbType.VarChar;
            cmd.Parameters.AddWithValue("LockTimeout", 1000);

            var returnValue = cmd.Parameters.Add("ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            await cmd.ExecuteNonQueryAsync(cancellation);

            return (int) returnValue.Value;
        }

        public static async Task<bool> TryGetGlobalTxLock(this SqlConnection conn, SqlTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Transaction", tx, cancellation) >= 0;
        }


        public static Task GetGlobalLock(this SqlConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            SqlTransaction transaction = null)
        {
            return getLock(conn, lockId, "Session", transaction, cancellation);
        }

        public static async Task<bool> TryGetGlobalLock(this SqlConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", null, cancellation) >= 0;
        }

        public static async Task<bool> TryGetGlobalLock(this SqlConnection conn, int lockId, SqlTransaction tx,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", tx, cancellation) >= 0;
        }

        public static Task ReleaseGlobalLock(this SqlConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            SqlTransaction tx = null)
        {
            var sqlCommand = new SqlCommand("sp_releaseapplock", conn, tx)
            {
                CommandType = CommandType.StoredProcedure
            };

            sqlCommand.Parameters.AddWithValue("Resource", lockId.ToString());
            sqlCommand.Parameters.AddWithValue("LockOwner", "Session");

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
