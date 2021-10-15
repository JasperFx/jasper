using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Weasel.Core;
using DbCommandBuilder = System.Data.Common.DbCommandBuilder;

namespace Jasper.Persistence.Database
{
    public abstract class DatabaseSettings
    {
        private string _schemaName;

        protected DatabaseSettings(string defaultSchema)
        {
            SchemaName = defaultSchema;
        }

        public string ConnectionString { get; set; }

        public string SchemaName
        {
            get => _schemaName;
            set
            {
                _schemaName = value;

                IncomingFullName = $"{value}.{DatabaseConstants.IncomingTable}";
                OutgoingFullName = $"{value}.{DatabaseConstants.OutgoingTable}";
                DeadLetterFullName = $"{value}.{DatabaseConstants.DeadLetterTable}";
            }
        }

        public string DeadLetterFullName { get; private set; }

        public string OutgoingFullName { get; private set; }

        public string IncomingFullName { get; private set; }

        public abstract DbConnection CreateConnection();

        public DbCommand CreateCommand(string command)
        {
            var cmd = CreateConnection().CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }

        public abstract DbCommand CreateEmptyCommand();

        public DbCommand CallFunction(string functionName)
        {
            var cmd = CreateConnection().CreateCommand();
            cmd.CommandText = SchemaName + "." + functionName;

            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public void ExecuteSql(string sql)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                conn.RunSql(sql);
            }
        }

        public Weasel.Core.DbCommandBuilder ToCommandBuilder()
        {
            return CreateConnection().ToCommandBuilder();
        }


        public abstract Task GetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId, CancellationToken cancellation = default(CancellationToken));

        public abstract Task<bool> TryGetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken));

        public abstract Task GetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction transaction = null);

        public abstract Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken));

        public abstract Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, DbTransaction tx,
            CancellationToken cancellation = default(CancellationToken));

        public abstract Task ReleaseGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null);
    }
}
