using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;

namespace Servers
{
    public class Containers : IDisposable
    {
        public static readonly Containers Instance = new Containers();

        private readonly object _locker = new object();
        private readonly IDockerClient _client = DockerServers.BuildDockerClient();

        private readonly Dictionary<Type, DockerServer> _containers = new Dictionary<Type, DockerServer>();

        public Task Start<T>() where T : DockerServer, new()
        {
            if (_containers.ContainsKey(typeof(T))) return Task.CompletedTask;

            DockerServer server;
            lock (_locker)
            {
                if (_containers.ContainsKey(typeof(T))) return Task.CompletedTask;

                server = new T();

                _containers.Add(typeof(T), server);
            }

            return server.Start(_client);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
