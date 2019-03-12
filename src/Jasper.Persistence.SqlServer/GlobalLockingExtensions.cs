using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer
{
    public static class GlobalLockingExtensions
    {
        public static Task GetGlobalTxLock(this SqlConnection conn, SqlTransaction tx, int lockId)
        {
            return getLock(conn, lockId, "Transaction", tx);
        }

        private static async Task getLock(SqlConnection conn, int lockId, string owner, SqlTransaction tx)
        {
            var returnValue = await tryGetLock(conn, lockId, owner, tx);

            if (returnValue < 0)
                throw new Exception(string.Format("sp_getapplock failed with errorCode '{0}'",
                    returnValue));
        }

        private static async Task<int> tryGetLock(SqlConnection conn, int lockId, string owner, SqlTransaction tx)
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
            await cmd.ExecuteNonQueryAsync();

            return (int) returnValue.Value;
        }

        public static async Task<bool> TryGetGlobalTxLock(this SqlConnection conn, SqlTransaction tx, int lockId)
        {
            return await tryGetLock(conn, lockId, "Transaction", tx) >= 0;
        }


        public static Task GetGlobalLock(this SqlConnection conn, int lockId, SqlTransaction transaction = null)
        {
            return getLock(conn, lockId, "Session", transaction);
        }

        public static async Task<bool> TryGetGlobalLock(this SqlConnection conn, int lockId)
        {
            return await tryGetLock(conn, lockId, "Session", null) >= 0;
        }

        public static async Task<bool> TryGetGlobalLock(this SqlConnection conn, int lockId, SqlTransaction tx)
        {
            return await tryGetLock(conn, lockId, "Session", tx) >= 0;
        }

        public static Task ReleaseGlobalLock(this SqlConnection conn, int lockId, SqlTransaction tx = null)
        {
            var sqlCommand = new SqlCommand("sp_releaseapplock", conn, tx)
            {
                CommandType = CommandType.StoredProcedure
            };

            sqlCommand.Parameters.AddWithValue("Resource", lockId.ToString());
            sqlCommand.Parameters.AddWithValue("LockOwner", "Session");

            return sqlCommand.ExecuteNonQueryAsync();
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
