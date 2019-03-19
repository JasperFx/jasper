using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Npgsql;

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
            throw new System.NotImplementedException();
        }

        public override Task<bool> TryGetGlobalTxLock(DbConnection conn, DbTransaction tx, int lockId,
            CancellationToken cancellation = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public override Task GetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction transaction = null)
        {
            throw new System.NotImplementedException();
        }

        public override Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public override Task<bool> TryGetGlobalLock(DbConnection conn, int lockId, DbTransaction tx,
            CancellationToken cancellation = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public override Task ReleaseGlobalLock(DbConnection conn, int lockId, CancellationToken cancellation = default(CancellationToken),
            DbTransaction tx = null)
        {
            throw new System.NotImplementedException();
        }
    }


}
