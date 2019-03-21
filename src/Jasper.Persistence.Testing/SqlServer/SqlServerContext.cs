using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    [Collection("sqlserver")]
    public abstract class SqlServerContext
    {
        protected SqlServerContext()
        {
            var loader = new SqlServerEnvelopeStorageAdmin(new SqlServerSettings{ConnectionString = Servers.SqlServerConnectionString});
            loader.RecreateAll();
        }
    }
}
