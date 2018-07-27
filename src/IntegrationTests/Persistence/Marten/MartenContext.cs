using Servers;
using Xunit;

namespace IntegrationTests.Persistence.Marten
{
    public abstract class MartenContext : IClassFixture<DockerFixture<MartenContainer>>
    {
        public MartenContext(DockerFixture<MartenContainer> fixture)
        {
        }
    }
}
