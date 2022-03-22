using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;

namespace Jasper.Persistence.Database
{
    public class DurableStorageSession : IDatabaseSession, IDurableStorageSession
    {
        private readonly DatabaseSettings _settings;
        private readonly CancellationToken _cancellation;

        public DurableStorageSession(DatabaseSettings settings, CancellationToken cancellation)
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

        public Task ReleaseNodeLock(int lockId)
        {
            return _settings.ReleaseGlobalLock(Connection, lockId, _cancellation);
        }

        public Task GetNodeLock(int lockId)
        {
            return _settings.GetGlobalLock(Connection, lockId, _cancellation);
        }

        public Task<bool> TryGetGlobalTxLock(int lockId)
        {
            return _settings.TryGetGlobalTxLock(Connection, Transaction, lockId, _cancellation);
        }

        public Task<bool> TryGetGlobalLock(int lockId)
        {
            return _settings.TryGetGlobalLock(Connection, lockId, Transaction, _cancellation);
        }

        public Task ReleaseGlobalLock(int lockId)
        {
            return _settings.ReleaseGlobalLock(Connection, lockId, _cancellation, Transaction);
        }

        public bool IsConnected()
        {
            return Connection?.State == ConnectionState.Open;
        }

        public async Task ConnectAndLockCurrentNode(ITransportLogger? logger, int nodeId)
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
                Connection = _settings.CreateConnection();

                await Connection.OpenAsync(_cancellation);

                await _settings.GetGlobalLock(Connection, nodeId, _cancellation, Transaction);
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
            Connection.Close();
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}
