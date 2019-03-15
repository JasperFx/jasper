using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Persistence.SqlServer.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{
    public abstract class DatabaseSettings
    {
        protected DatabaseSettings(string defaultSchema)
        {
            SchemaName = defaultSchema;
        }

        public string SchemaName { get; set; }

        public abstract DbConnection CreateConnection();

        public DbCommand CreateCommand(string command)
        {
            var cmd = CreateConnection().CreateCommand();
            cmd.CommandText = command;

            return cmd;
        }

        public DbCommand CallFunction(string functionName)
        {
            var cmd = CreateConnection().CreateCommand();
            cmd.CommandText = SchemaName + "." + functionName;

            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        public void ExecuteSql(string sql)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                conn.RunSql(sql);
            }
        }
    }


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
