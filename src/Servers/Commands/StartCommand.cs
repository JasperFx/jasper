using System;
using System.Threading.Tasks;
using Oakton;
using Servers.Docker;

namespace Servers.Commands
{
    public enum Server
    {
        marten,
        sqlserver,
        consul,
        rabbitmq
    }

    public class DockerInput
    {
        internal DockerServer[] SelectServers()
        {
            var marten = new MartenContainer();
            var sqlServer = new SqlServerContainer();
            var consul = new ConsulContainer();
            var rabbitmq = new RabbitMQContainer();

            return new DockerServer[] {marten, sqlServer, consul, rabbitmq};
        }
    }

    [Oakton.Description("Starts all or designated docker containers")]
    public class StartCommand : OaktonAsyncCommand<DockerInput>
    {
        public override async Task<bool> Execute(DockerInput input)
        {
            using (var client = DockerServers.BuildDockerClient())
            {

                foreach (var server in input.SelectServers())
                {
                    await server.Start(client);
                    Console.WriteLine($"{server} started: {server.StartAction}");
                }

            }

            return true;
        }
    }

    [Oakton.Description("Stops all or designated docker containers")]
    public class StopCommand : OaktonAsyncCommand<DockerInput>
    {
        public override async Task<bool> Execute(DockerInput input)
        {
            using (var client = DockerServers.BuildDockerClient())
            {

                foreach (var server in input.SelectServers())
                {
                    await server.Stop(client);
                    Console.WriteLine($"Stopped {server}");
                }

            }

            return true;
        }
    }
}
