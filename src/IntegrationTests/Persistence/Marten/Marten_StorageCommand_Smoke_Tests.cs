using Jasper;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public class Marten_StorageCommand_Smoke_Tests : MartenContext
    {
        [Theory]
        [InlineData("storage rebuild")]
        [InlineData("storage counts")]
        [InlineData("storage script")]
        [InlineData("storage clear")]
        public void smoke_test_calls(string commandLine)
        {
            var registry = new JasperRegistry();
            registry.MartenConnectionStringIs(Servers.PostgresConnectionString);
            registry.Include<MartenBackedPersistence>();

            var args = commandLine.Split(' ');
            JasperHost.Run(registry, args).ShouldBe(0);
        }



    }
}
