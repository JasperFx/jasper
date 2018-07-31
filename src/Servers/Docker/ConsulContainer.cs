using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Servers.Docker
{
    public class ConsulContainer : DockerServer
    {
        public ConsulContainer() : base("consul:latest", "jasper-consul")
        {
        }

        protected override async Task<bool> isReady()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://localhost:8500");
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override HostConfig ToHostConfig()
        {
            return new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "8500/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = $"8500",
                                HostIP = "127.0.0.1"
                            }
                        }
                    },
                    {
                        "8600/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = $"8600",
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
                Env = new List<string> {"CONSUL_BIND_INTERFACE=eth0", "CONSUL_CLIENT_INTERFACE=eth0"},
                Cmd = new List<string>{"agent", "-dev", "-node", "local"}

            };
        }
    }
}
