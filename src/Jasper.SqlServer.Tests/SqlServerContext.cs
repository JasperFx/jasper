using Jasper.SqlServer.Schema;
using Servers;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public abstract class SqlServerContext : IClassFixture<DockerFixture<SqlServerContainer>>
    {
        protected SqlServerContext(DockerFixture<SqlServerContainer> fixture)
        {
            var loader = new SchemaLoader(SqlServerContainer.ConnectionString);
            loader.RecreateAll();
        }
    }
}
