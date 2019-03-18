using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Persistence.Database;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableStorageSession : IDatabaseSession, IDurableStorageSession
    {
        private readonly SqlServerSettings _settings;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableStorageSession(SqlServerSettings settings, CancellationToken cancellation)
        {
            _settings = settings;
            _cancellation = cancellation;
        }

        public DbTransaction Transaction { get; private set; }

        public DbConnection Connection { get; private set; }

        public DbCommand CreateCommand(string sql)
        {
            var cmd = Connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = Transaction;

            return cmd;
        }

        public DbCommand CallFunction(string functionName)
        {
            var cmd = CreateCommand(_settings.SchemaName + "." + functionName);
            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public Task ReleaseNodeLock(int lockId)
        {
            return Connection.ReleaseGlobalLock(lockId, _cancellation);
        }

        public Task GetNodeLock(int lockId)
        {
            return Connection.GetGlobalLock(lockId, _cancellation);
        }

        public Task Begin()
        {
            Transaction = Connection.BeginTransaction();
            return Task.CompletedTask;
        }

        public Task Commit()
        {
            Transaction.Commit();
            Transaction = null;
            return Task.CompletedTask;
        }

        public Task Rollback()
        {
            Transaction.Rollback();
            return Task.CompletedTask;
        }

        public Task<bool> TryGetGlobalTxLock(int lockId)
        {
            return Connection.TryGetGlobalTxLock(Transaction, lockId, _cancellation);
        }

        public Task<bool> TryGetGlobalLock(int lockId)
        {
            return Connection.TryGetGlobalLock(lockId, Transaction, _cancellation);
        }

        public Task ReleaseGlobalLock(int lockId)
        {
            return Connection.ReleaseGlobalLock(lockId, _cancellation, Transaction);
        }

        public bool IsConnected()
        {
            return Connection?.State == ConnectionState.Open;
        }

        public async Task ConnectAndLockCurrentNode(ITransportLogger logger, int nodeId)
        {
            if (Connection != null)
            {
                try
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
                catch (Exception e)
                {
                    logger.LogException(e);
                }
            }

            try
            {
                Connection = new SqlConnection(_settings.ConnectionString);

                // TODO -- use the CancellationToken from JasperSettings
                await Connection.OpenAsync(_cancellation);

                await Connection.GetGlobalLock(nodeId, _cancellation, Transaction);
            }
            catch (Exception e)
            {
                Connection?.Dispose();
                Connection = null;

                throw;
            }
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}
