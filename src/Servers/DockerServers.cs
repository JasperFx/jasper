using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace Servers
{
    public class DockerServers
    {
        public static IDockerClient BuildDockerClient()
        {
            var uriString = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";

            Console.WriteLine($"Connecting to the Docker daemon at '{uriString}'");

            var config = new DockerClientConfiguration(new Uri(uriString));

            return config.CreateClient();
        }
    }
}