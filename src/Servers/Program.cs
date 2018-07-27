using System;
using System.Threading.Tasks;
using Oakton;

namespace Servers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new SqlServerContainer();

            var client = DockerServers.BuildDockerClient();

            await server.Start(client);

            ConsoleWriter.Write(ConsoleColor.Green, "Looking good folks!");

            await server.Stop(client);

            Console.WriteLine("Shut down okay");
        }


    }
}
