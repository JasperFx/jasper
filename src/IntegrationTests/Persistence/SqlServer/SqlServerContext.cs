using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Schema;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
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
