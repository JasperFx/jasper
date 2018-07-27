using Servers;
using Xunit;

namespace Jasper.Marten.Tests
{
    public abstract class MartenContext : IClassFixture<DockerFixture<MartenContainer>>
    {
        public MartenContext(DockerFixture<MartenContainer> fixture)
        {
        }
    }
}
