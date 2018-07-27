using Servers;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public abstract class SqlServerContext : IClassFixture<DockerFixture<SqlServerContainer>>
    {
        protected SqlServerContext(DockerFixture<SqlServerContainer> fixture)
        {
        }
    }
}