using System.Data.Common;
using System.Data.SqlClient;
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


    }
}
