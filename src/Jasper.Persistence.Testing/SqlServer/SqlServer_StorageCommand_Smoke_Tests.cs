using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.Hosting;
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
            (await Host.CreateDefaultBuilder().UseJasper(registry =>
            {
                registry.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);
            }).RunJasper(args)).ShouldBe(0);
        }

    }
}
