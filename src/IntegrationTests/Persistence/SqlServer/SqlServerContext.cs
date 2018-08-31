using Jasper.Persistence.SqlServer.Schema;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
{
    [Collection("sqlserver")]
    public abstract class SqlServerContext
    {
        protected SqlServerContext()
        {
            var loader = new SchemaLoader(Servers.SqlServerConnectionString);
            loader.RecreateAll();
        }
    }
}
