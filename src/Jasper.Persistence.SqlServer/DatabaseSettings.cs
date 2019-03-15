using System.Data;
using System.Data.Common;
using Jasper.Persistence.SqlServer.Util;

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
}