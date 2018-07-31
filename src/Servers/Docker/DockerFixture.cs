using Baseline.Dates;
using Docker.DotNet;

namespace Servers.Docker
{
    public class DockerFixture<T> where T : DockerServer, new()
    {
        private readonly IDockerClient _client;
        private readonly T _server;

        public DockerFixture()
        {
            var start = Containers.Instance.Start<T>();
            start.Wait(5.Seconds());

        }
    }
}
