using System.Data;
using System.Data.SqlClient;
using Jasper.Persistence.SqlServer.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{


    public class SqlServerSettings
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; } = "dbo";

        /// <summary>
        ///     The value of the 'database_principal' parameter in calls to APPLOCK_TEST
        /// </summary>
        public string DatabasePrincipal { get; set; } = "dbo";

        public SqlCommand CreateCommand(string command)
        {
            var cmd = new SqlConnection(ConnectionString).CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }

        public SqlCommand CallFunction(string functionName)
        {
            var cmd = new SqlConnection(ConnectionString).CreateCommand();
            cmd.CommandText = SchemaName + "." + functionName;

            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public void ExecuteSql(string sql)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                conn.RunSql(sql);
            }
        }
    }
}
