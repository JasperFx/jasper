using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Microsoft.Data.SqlClient;
using Weasel.Core;
using Weasel.SqlServer;

namespace Jasper.Persistence.SqlServer
{
    public class SqlServerSettings : DatabaseSettings
    {
        public SqlServerSettings() : base("dbo", new SqlServerMigrator())
        {
        }




        /// <summary>
        ///     The value of the 'database_principal' parameter in calls to APPLOCK_TEST
        /// </summary>
        public string DatabasePrincipal { get; set; } = "dbo";

        public override DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public override DbCommand CreateEmptyCommand()
        {
            return new SqlCommand();
        }

        public override Task GetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return getLock(conn, lockId, "Transaction", tx, cancellation);
        }

        private static async Task getLock(DbConnection conn, int lockId, string owner, DbTransaction tx,
            CancellationToken cancellation)
        {
            var returnValue = await tryGetLock(conn, lockId, owner, tx, cancellation);

            if (returnValue < 0)
                throw new Exception($"sp_getapplock failed with errorCode '{returnValue}'");
        }

        private static async Task<int> tryGetLock(DbConnection conn, int lockId, string owner, DbTransaction tx,
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

        public override async Task<bool> TryGetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Transaction", tx, cancellation) >= 0;
        }


        public override Task GetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction transaction = null)
        {
            return getLock(conn, lockId, "Session", transaction, cancellation);
        }

        public override async Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", null, cancellation) >= 0;
        }

        public override async Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, DbTransaction tx,
            CancellationToken cancellation = default(CancellationToken))
        {
            return await tryGetLock(conn, lockId, "Session", tx, cancellation) >= 0;
        }

        public override Task ReleaseGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null)
        {
            var sqlCommand = conn.CreateCommand("sp_releaseapplock");
            sqlCommand.Transaction = tx;
            sqlCommand.CommandType = CommandType.StoredProcedure;

            sqlCommand.With("Resource", lockId.ToString());
            sqlCommand.With("LockOwner", "Session");

            return sqlCommand.ExecuteNonQueryAsync(cancellation);
        }
    }
}
