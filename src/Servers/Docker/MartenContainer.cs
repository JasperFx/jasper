using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Npgsql;

namespace Servers.Docker
{
    public class MartenContainer : DockerServer
    {
        public MartenContainer() : base("clkao/postgres-plv8:latest", "jasper-postgresql")
        {
        }

        public static readonly string ConnectionString =
            "Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres";

        protected override async Task<bool> isReady()
        {
            try
            {
                using (var conn =
                    new NpgsqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();

                    return true;
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
                        "5432/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = $"5433",
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
                Env = new List<string> {"POSTGRES_PASSWORD=postgres"}
            };
        }
    }
}
