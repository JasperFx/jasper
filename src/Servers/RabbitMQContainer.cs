using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using RabbitMQ.Client;

namespace Servers
{
    public class RabbitMQContainer : DockerServer
    {
        public RabbitMQContainer() : base("rabbitmq", "jasper-rabbitmq")
        {
        }

        protected override Task<bool> isReady()
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.Port = 5672;
            try
            {
                using (var conn = factory.CreateConnection())
                {
                    return Task.FromResult(true);
                }
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public override HostConfig ToHostConfig()
        {
            return new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "5672/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = $"5672",
                                HostIP = "127.0.0.1"
                            }
                        }
                    },


                },

            };
        }

        public override Config ToConfig()
        {
            return new Config
            {
                Hostname = "JasperRabbitMq"
            };
        }
    }
}
