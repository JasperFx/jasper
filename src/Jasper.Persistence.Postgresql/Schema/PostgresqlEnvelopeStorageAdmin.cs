using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql.Util;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql.Schema
{
    public class PostgresqlEnvelopeStorageAdmin : DataAccessor, IEnvelopeStorageAdmin
    {
        private readonly string _connectionString;

        private readonly string[] _creationOrder =
        {
            "Creation.sql"
        };

        public PostgresqlEnvelopeStorageAdmin(PostgresqlSettings settings)
        {
            _connectionString = settings.ConnectionString;
            SchemaName = settings.SchemaName;
        }

        public string SchemaName { get; set; } = "public";

        private static string toScript(string fileName, string schema)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(PostgresqlEnvelopeStorageAdmin), fileName)
                .ReadAllText();

            return text.Replace("%SCHEMA%", schema);
        }

        public void DropAll()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");
            }
        }

        private void execute(NpgsqlConnection conn, string filename)
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        public void RecreateAll()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        private void buildSchemaIfNotExists(NpgsqlConnection conn)
        {
            conn.CreateCommand($"CREATE SCHEMA IF NOT EXISTS {SchemaName};").ExecuteNonQuery();
        }

        void IEnvelopeStorageAdmin.ClearAllPersistedEnvelopes()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    conn.CreateCommand($"truncate table {SchemaName}.{OutgoingTable};truncate table {SchemaName}.{IncomingTable};truncate table {SchemaName}.{DeadLetterTable};").ExecuteNonQuery();
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
            writer.WriteLine($"CREATE SCHEMA IF NOT EXISTS {SchemaName};");
            writer.WriteLine();

            foreach (var file in _creationOrder)
            {
                var sql = toScript(file, SchemaName);
                writer.WriteLine(sql);
                writer.WriteLine();
            }


            return writer.ToString();
        }

        public async Task<PersistedCounts> GetPersistedCounts()
        {
            var counts = new PersistedCounts();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();


                using (var reader = await conn
                    .CreateCommand(
                        $"select status, count(*) from {SchemaName}.{IncomingTable} group by status")
                    .ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var status = (EnvelopeStatus)Enum.Parse(typeof(EnvelopeStatus), await reader.GetFieldValueAsync<string>(0));
                        var count = await reader.GetFieldValueAsync<int>(1);

                        if (status == EnvelopeStatus.Incoming)
                            counts.Incoming = count;
                        else if (status == EnvelopeStatus.Scheduled) counts.Scheduled = count;
                    }
                }

                var longCount = await conn
                    .CreateCommand($"select count(*) from {SchemaName}.{OutgoingTable}").ExecuteScalarAsync();

                counts.Outgoing =  Convert.ToInt32(longCount);
            }


            return counts;
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand(
                    $"select body, explanation, exception_text, exception_type, exception_message, source, message_type, id from {SchemaName}.{DeadLetterTable} where id = @id");
                cmd.With("id", id, NpgsqlDbType.Uuid);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync()) return null;


                    var report = new ErrorReport
                    {
                        RawData = await reader.GetFieldValueAsync<byte[]>(0),
                        Explanation = await reader.GetFieldValueAsync<string>(1),
                        ExceptionText = await reader.GetFieldValueAsync<string>(2),
                        ExceptionType = await reader.GetFieldValueAsync<string>(3),
                        ExceptionMessage = await reader.GetFieldValueAsync<string>(4),
                        Source = await reader.GetFieldValueAsync<string>(5),
                        MessageType = await reader.GetFieldValueAsync<string>(6),
                        Id = await reader.GetFieldValueAsync<Guid>(7)
                    };

                    return report;
                }
            }
        }

        public async Task<Envelope[]> AllIncomingEnvelopes()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, status, owner_id from {SchemaName}.{IncomingTable}").ExecuteToEnvelopes();
            }
        }

        public async Task<Envelope[]> AllOutgoingEnvelopes()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, '{EnvelopeStatus.Outgoing}', owner_id from {SchemaName}.{OutgoingTable}").ExecuteToEnvelopes();
            }
        }

        public void ReleaseAllOwnership()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                conn.CreateCommand($"update {SchemaName}.{IncomingTable} set owner_id = 0;update {SchemaName}.{OutgoingTable} set owner_id = 0");
            }
        }
    }
}
