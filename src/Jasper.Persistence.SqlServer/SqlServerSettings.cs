using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Persistence.SqlServer.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{
    public class SqlServerSettings : DatabaseSettings
    {
        public SqlServerSettings() : base("dbo")
        {
        }

        public string ConnectionString { get; set; }


        /// <summary>
        ///     The value of the 'database_principal' parameter in calls to APPLOCK_TEST
        /// </summary>
        public string DatabasePrincipal { get; set; } = "dbo";

        public override DbConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public override DbCommand CreateEmptyCommand()
        {
            return new SqlCommand();
        }


        public CommandBuilder ToCommandBuilder()
        {
            var cmd = CreateConnection().CreateCommand("");
            return new CommandBuilder(cmd);
        }


    }
}
