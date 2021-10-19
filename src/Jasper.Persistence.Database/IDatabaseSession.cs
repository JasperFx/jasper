using System.Data.Common;

namespace Jasper.Persistence.Database
{
    public interface IDatabaseSession
    {
        public DbTransaction Transaction { get; }
        DbConnection Connection { get; }
        DbCommand CreateCommand(string sql);
        public DbCommand CallFunction(string functionName);
    }
}