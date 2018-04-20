using System;
using System.Data.SqlClient;
using System.Reflection;
using Baseline;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Schema
{
    public class SchemaLoader
    {
        private readonly string _connectionString;

        public SchemaLoader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string SchemaName { get; set; } = "dbo";

        public static string ToCreationScript(string schema)
        {
            return toScript("Creation.sql", schema);
        }

        public static string ToDropScript(string schema)
        {
            return toScript("Drop.sql", schema);
        }

        private static string toScript(string fileName, string schema)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(SchemaLoader), fileName)
                .ReadAllText();

            return text.Replace("%SCHEMA%", schema);
        }

        public void DropAll()
        {
            var sql = ToDropScript(SchemaName);
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                conn.CreateCommand(sql).ExecuteNonQuery();
            }
        }

        public void CreateAll()
        {
            var sql = ToCreationScript(SchemaName);
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                conn.CreateCommand(sql).ExecuteNonQuery();
            }
        }

        public void RecreateAll()
        {
            var drop = ToDropScript(SchemaName);
            var create = ToCreationScript(SchemaName);
            var sql = drop + create;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                conn.CreateCommand(sql).ExecuteNonQuery();
            }
        }
    }
}
