using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Servers
{
    public class RabbitMQContainer : DockerServer
    {
        public RabbitMQContainer() : base("rabbitmq", "jasper-rabbitmq")
        {
        }

        protected override Task<bool> isReady()
        {
            return Task.FromResult(true);
        }

        public override HostConfig ToHostConfig()
        {
            return new HostConfig();
        }

        public override Config ToConfig()
        {
            return new Config();
        }
    }
}
