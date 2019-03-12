using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Persistence.Postgresql.Util;
using Npgsql;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableStorageSession : IDurableStorageSession
    {
        private readonly CancellationToken _cancellation;
        private readonly string _connectionString;

        public PostgresqlDurableStorageSession(PostgresqlSettings settings, CancellationToken cancellation)
        {
            _cancellation = cancellation;
            _connectionString = settings.ConnectionString;
        }


        internal NpgsqlTransaction Transaction { get; private set; }

        internal NpgsqlConnection Connection { get; private set; }

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
            return Transaction.TryGetGlobalTxLock(lockId, _cancellation);
        }

        public Task<bool> TryGetGlobalLock(int lockId)
        {
            return Connection.TryGetGlobalLock(lockId, _cancellation);
        }

        public Task ReleaseGlobalLock(int lockId)
        {
            return Connection.ReleaseGlobalLock(lockId, _cancellation);
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
                Connection = new NpgsqlConnection(_connectionString);

                await Connection.OpenAsync(_cancellation);

                await Connection.GetGlobalLock(nodeId, _cancellation);
            }
            catch (Exception)
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

        public NpgsqlCommand CreateCommand(string sql)
        {
            if (Connection == null) throw new InvalidOperationException("Connection has not been opened on the session");
            return new NpgsqlCommand(sql, Connection, Transaction);

        }
    }
}
