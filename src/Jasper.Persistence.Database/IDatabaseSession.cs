using System.Data.Common;

namespace Jasper.Persistence.Database
{
    public interface IDatabaseSession
    {
        DbTransaction Transaction { get; }
        DbConnection Connection { get; }
        DbCommand CreateCommand(string sql);
        DbCommand CallFunction(string functionName);
    }
}