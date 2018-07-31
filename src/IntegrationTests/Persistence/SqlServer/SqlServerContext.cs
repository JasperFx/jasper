using Jasper.Persistence.SqlServer.Schema;
using Servers;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
{
    [Collection("sqlserver")]
    public abstract class SqlServerContext : IClassFixture<DockerFixture<SqlServerContainer>>
    {
        protected SqlServerContext(DockerFixture<SqlServerContainer> fixture)
        {
            var loader = new SchemaLoader(SqlServerContainer.ConnectionString);
            loader.RecreateAll();
        }
    }
}
