using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Schema
{
    public class SqlServerEnvelopeStorageAdmin : IEnvelopeStorageAdmin
    {
        private readonly string _connectionString;

        private readonly string[] _creationOrder =
        {
            "Creation.sql",
            "uspDeleteIncomingEnvelopes.sql",
            "uspDeleteOutgoingEnvelopes.sql",
            "uspDiscardAndReassignOutgoing.sql",
            "uspMarkIncomingOwnership.sql",
            "uspMarkOutgoingOwnership.sql"
        };

        public SqlServerEnvelopeStorageAdmin(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlServerEnvelopeStorageAdmin(string connectionString, string schemaName)
        {
            _connectionString = connectionString;
            SchemaName = schemaName;
        }

        public string SchemaName { get; set; } = "dbo";

        public static string ToCreationScript(string schema)
        {
            // TODO -- more here
            return toScript("Creation.sql", schema);
        }

        public static string ToDropScript(string schema)
        {
            return toScript("Drop.sql", schema);
        }

        private static string toScript(string fileName, string schema)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(SqlServerEnvelopeStorageAdmin), fileName)
                .ReadAllText();

            return text.Replace("%SCHEMA%", schema);
        }

        public void DropAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");
            }
        }

        private void execute(SqlConnection conn, string filename)
        {
            var sql = toScript(filename, SchemaName);

            try
            {
                conn.CreateCommand(sql).ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failure trying to execute:\n\n" + sql, e);
            }
        }

        public void CreateAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        public void RecreateAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        private void buildSchemaIfNotExists(SqlConnection conn)
        {
            var count = conn.CreateCommand("select count(*) from sys.schemas where name = @name")
                .With("name", SchemaName).ExecuteScalar().As<int>();

            if (count == 0) conn.CreateCommand($"CREATE SCHEMA [{SchemaName}] AUTHORIZATION [dbo]").ExecuteNonQuery();
        }

        void IEnvelopeStorageAdmin.ClearAllPersistedEnvelopes()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    conn.CreateCommand($"truncate table {SchemaName}.jasper_outgoing_envelopes;truncate table {SchemaName}.jasper_incoming_envelopes;truncate table {SchemaName}.jasper_dead_letters;").ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Failure trying to execute the truncate table statements for the envelope storage", e);
                }

            }
        }

        void IEnvelopeStorageAdmin.RebuildSchemaObjects()
        {
            DropAll();
            RecreateAll();
        }

        string IEnvelopeStorageAdmin.CreateSql()
        {
            var writer = new StringWriter();
            writer.WriteLine(
                $@"
IF EXISTS (SELECT name FROM sys.schemas WHERE name = N'{SchemaName}')
   BEGIN
      PRINT 'Dropping the {SchemaName} schema'
      DROP SCHEMA [{SchemaName}]
END
GO
PRINT '    Creating the {SchemaName} schema'
GO
CREATE SCHEMA [{SchemaName}] AUTHORIZATION [dbo]
GO

");

            foreach (var file in _creationOrder)
            {
                var sql = toScript(file, SchemaName);
                writer.WriteLine(sql);
                writer.WriteLine("GO");
                writer.WriteLine();
            }



            return writer.ToString();

        }
    }
}
