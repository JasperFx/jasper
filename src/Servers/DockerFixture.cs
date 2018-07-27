using System;
using Baseline.Dates;
using Docker.DotNet;

namespace Servers
{
    public class DockerFixture<T> : IDisposable where T : DockerServer, new()
    {
        private readonly IDockerClient _client;
        private readonly T _server;
        private readonly StartAction _ownership;

        public DockerFixture()
        {
            _client = DockerServers.BuildDockerClient();
            _server = new T();

            var start = _server.Start(_client);
            start.Wait(5.Seconds());

            _ownership = start.Result;
        }


        public void Dispose()
        {
            if (_ownership == StartAction.started)
            {
                _server.Stop(_client).Wait(5.Seconds());
            }

            _client.Dispose();
        }
    }
}
