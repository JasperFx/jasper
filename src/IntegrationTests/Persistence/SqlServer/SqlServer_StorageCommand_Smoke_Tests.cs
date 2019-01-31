using Jasper;
using Jasper.Persistence.SqlServer;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
{
    public class SqlServer_StorageCommand_Smoke_Tests : SqlServerContext
    {
        [Theory]
        [InlineData("storage rebuild")]
        [InlineData("storage counts")]
        [InlineData("storage script")]
        [InlineData("storage clear")]
        public void smoke_test_calls(string commandLine)
        {
            var registry = new JasperRegistry();
            registry.Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
s
            var args = commandLine.Split(' ');
            JasperHost.Run(registry, args).ShouldBe(0);
        }

    }
}
