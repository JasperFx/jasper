using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class Marten_StorageCommand_Smoke_Tests : PostgresqlContext
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

            var exitCode = await JasperHost.Run(args, registry =>
            {
                registry.MartenConnectionStringIs(Servers.PostgresConnectionString);
                registry.Include<MartenBackedPersistence>();
            });

            exitCode.ShouldBe(0);
        }



    }
}
