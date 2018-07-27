using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Servers
{
    public class SqlServer : DockerServer
    {
        public SqlServer() : base("microsoft/mssql-server-linux:2017-latest", "jasper-mssql")
        {
        }

        public static readonly string ConnectionString = "Server=localhost;User Id=sa;Password=P@55w0rd;Timeout=5";

        protected override async Task<bool> isReady()
        {
            try
            {
                using (var conn =
                    new SqlConnection("Server=localhost;User Id=sa;Password=P@55w0rd;Timeout=5"))
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
                        "1433/tcp",
                        new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = $"1433",
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
                Env = new List<string> {"ACCEPT_EULA=Y", "SA_PASSWORD=P@55w0rd", "MSSQL_PID=Developer"}
            };
        }
    }
}
