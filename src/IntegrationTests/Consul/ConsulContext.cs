using Servers.Docker;
using Xunit;

namespace IntegrationTests.Consul
{
    [Collection("consul")]
    public class ConsulContext : IClassFixture<DockerFixture<ConsulContainer>>
    {
        public ConsulContext(DockerFixture<ConsulContainer> fixture)
        {
        }
    }
}
