using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    public class SqlServer_StorageCommand_Smoke_Tests : SqlServerContext
    {
        [Theory]
        [InlineData("storage rebuild")]
        [InlineData("storage counts")]
        [InlineData("storage script")]
        [InlineData("storage clear")]
        [InlineData("storage release")]
        public async Task smoke_test_calls(string commandLine)
        {
            var args = commandLine.Split(' ');
            (await JasperHost.Run(args, registry =>
            {
                registry.Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
            })).ShouldBe(0);
        }

    }
}
