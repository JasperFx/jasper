using Servers;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    public abstract class RabbitMQContext : IClassFixture<DockerFixture<RabbitMQContainer>>
    {
        public RabbitMQContext(DockerFixture<RabbitMQContainer> container)
        {
        }
    }
}
