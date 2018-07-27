using System;
using Baseline.Dates;
using Docker.DotNet;

namespace Servers
{
    public class DockerFixture<T> : IDisposable where T : DockerServer, new()
    {
        private readonly IDockerClient _client;
        private readonly T _server;

        public DockerFixture()
        {
            _client = DockerServers.BuildDockerClient();
            _server = new T();

            _server.Start(_client).Wait(5.Seconds());
        }


        public void Dispose()
        {
            _server.Stop(_client).Wait(5.Seconds());

            _client.Dispose();
        }
    }
}
