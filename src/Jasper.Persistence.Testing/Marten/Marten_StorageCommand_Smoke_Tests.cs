using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Microsoft.Extensions.Hosting;
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



            var exitCode = await Host.CreateDefaultBuilder().UseJasper(registry =>
            {
                registry.MartenConnectionStringIs(Servers.PostgresConnectionString);
                registry.Include<MartenBackedPersistence>();
            }).RunJasper(args);

            exitCode.ShouldBe(0);
        }



    }
}
