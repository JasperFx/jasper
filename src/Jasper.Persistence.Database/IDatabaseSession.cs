using System.Data.Common;

namespace Jasper.Persistence.Database
{
    public interface IDatabaseSession
    {
        public DbTransaction Transaction { get; }
        DbConnection Connection { get; }
        public DbCommand CallFunction(string functionName);
        DbCommand CreateCommand(string sql);
    }
}